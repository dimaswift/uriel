
import csv
from datetime import datetime, timedelta
from pathlib import Path
import numpy as np
from skyfield.api import load, Topos, utc
from skyfield.almanac import find_discrete, risings_and_settings
from skyfield.data import hipparcos
from skyfield.positionlib import Apparent

# Custom epoch: January 4, 1992, 23:05:37 UTC 694566337
CUSTOM_EPOCH = datetime(1992, 1, 4, 23, 5, 37, tzinfo=utc)
UNIX_EPOCH = datetime(1970, 1, 1, 0, 0, 0, tzinfo=utc)
CUSTOM_EPOCH_OFFSET = int((CUSTOM_EPOCH - UNIX_EPOCH).total_seconds())

# Event types for rise/set/culmination
EVENT_NONE = 0
EVENT_RISE = 1
EVENT_SET = 2
EVENT_CULMINATION = 3

# Distance event types for apogee/perigee
DISTANCE_EVENT_NONE = 0
DISTANCE_EVENT_PERIGEE = 1  # Closest approach
DISTANCE_EVENT_APOGEE = 2   # Farthest point

# Planet IDs for closest body detection
PLANET_MAP = {
    'mercury': 1, 'venus': 2, 'mars': 4, 'jupiter': 5, 'saturn': 6,
    'uranus': 7, 'neptune': 8, 'sun': 10, 'moon': 11, 'unknown': 0
}

class CelestialEphemerisGenerator:
    def __init__(self, observer_lat, observer_lon, observer_elevation=0):
        """
        Initialize the ephemeris generator.
        
        Args:
            observer_lat: Observer latitude in degrees
            observer_lon: Observer longitude in degrees  
            observer_elevation: Observer elevation in meters
        """
        self.observer = Topos(observer_lat, observer_lon, elevation_m=observer_elevation)
        self.observer_lat = observer_lat
        self.observer_lon = observer_lon
        self.observer_elevation = observer_elevation
        self.ts = load.timescale()
        self.planets = load('de421.bsp')  # JPL planetary ephemeris
        
        # Load celestial objects
        self.sun = self.planets['sun']
        self.moon = self.planets['moon']
        self.earth = self.planets['earth']
        
        # Available planets for closest body detection
        self.available_planets = {
            'mercury': self.planets['mercury'],
            'venus': self.planets['venus'],
            'mars': self.planets['mars'],
            'jupiter': self.planets['jupiter barycenter'],
            'saturn': self.planets['saturn barycenter'],
            'uranus': self.planets['uranus barycenter'],
            'neptune': self.planets['neptune barycenter'],
            'sun': self.sun,
            'moon': self.moon
        }
        
    def datetime_to_custom_epoch(self, dt):
        """Convert datetime to custom epoch timestamp."""
        # Ensure datetime is timezone-aware (UTC)
        if dt.tzinfo is None:
            dt = dt.replace(tzinfo=utc)
        unix_timestamp = dt.timestamp()
        return int(unix_timestamp - CUSTOM_EPOCH_OFFSET)
    
    def custom_epoch_to_datetime(self, custom_timestamp):
        """Convert custom epoch timestamp to datetime."""
        unix_timestamp = custom_timestamp + CUSTOM_EPOCH_OFFSET
        return datetime.fromtimestamp(unix_timestamp, tz=utc)
    
    def get_moon_phase(self, t):
        """Calculate moon phase (0.0 = new moon, 1.0 = full moon)."""
        earth = self.planets['earth']
        sun = self.planets['sun']
        moon = self.planets['moon']
        
        # Calculate phase angle using the elongation method
        earth_pos = earth.at(t)
        sun_apparent = earth_pos.observe(sun)
        moon_apparent = earth_pos.observe(moon)
        
        # Calculate elongation (angular separation between sun and moon as seen from Earth)
        elongation = sun_apparent.separation_from(moon_apparent)
        elongation_degrees = elongation.degrees
        
        # Convert elongation to phase (0 = new moon, 180 = full moon)
        # Phase ranges from 0.0 (new) to 1.0 (full)
        phase = (1.0 - np.cos(np.radians(elongation_degrees))) / 2.0
        
        return phase
    
    def find_closest_planet(self, target_body, t, current_body_name):
        """
        Find the closest planet to the target celestial body.
        Returns (planet_id, angular_distance_degrees)
        """
        earth_pos = self.earth.at(t)
        target_apparent = earth_pos.observe(target_body).apparent()
        
        closest_distance = float('inf')
        closest_planet_id = 0
        
        for planet_name, planet_body in self.available_planets.items():
            # Skip the current body itself
            if planet_name.lower() == current_body_name.lower():
                continue
                
            try:
                planet_apparent = earth_pos.observe(planet_body).apparent()
                separation = target_apparent.separation_from(planet_apparent)
                separation_degrees = separation.degrees
                
                if separation_degrees < closest_distance:
                    closest_distance = separation_degrees
                    closest_planet_id = PLANET_MAP.get(planet_name, 0)
                    
            except Exception as e:
                # Skip planets that can't be calculated
                continue
        
        return closest_planet_id, closest_distance
    
    def calculate_position_at_time(self, body, current_body_name, time_dt):
        """Calculate position data for a celestial body at a specific time."""
        t = self.ts.from_datetime(time_dt)
        observer_at_t = self.earth + self.observer
        apparent = observer_at_t.at(t).observe(body).apparent()
        
        ra, dec, distance = apparent.radec()
        alt, az, d = apparent.altaz()
        
        timestamp = self.datetime_to_custom_epoch(time_dt)
        distance_km = distance.km
        azimuth_deg = az.degrees
        altitude_deg = alt.degrees
        
        # Calculate phase (for moon only)
        if current_body_name.lower() == 'moon':
            phase = self.get_moon_phase(t)
        else:
            phase = 0.0
        
        # Find closest planet
        closest_planet_id, angular_distance = self.find_closest_planet(body, t, current_body_name)
        
        return {
            'timestamp': timestamp,
            'phase': phase,
            'distance': distance_km,
            'azimuth': azimuth_deg,
            'altitude': altitude_deg,
            'closest_planet_id': closest_planet_id,
            'angular_distance': angular_distance,
            'datetime_utc': time_dt.strftime('%Y-%m-%d %H:%M:%S')
        }
    
    def find_exact_event_time(self, body, current_body_name, start_time, end_time, event_type, precision_minutes=1):
        """
        Find the exact time of a rise or set event using smaller time steps.
        
        Args:
            body: Celestial body object
            current_body_name: Name of the celestial body
            start_time: Start time (datetime)
            end_time: End time (datetime)  
            event_type: EVENT_RISE or EVENT_SET
            precision_minutes: Time step in minutes for precision search
            
        Returns:
            Dictionary with exact event data, or None if not found
        """
        horizon = 0.0  # degrees
        time_delta = timedelta(minutes=precision_minutes)
        current_time = start_time
        
        closest_to_horizon = None
        closest_distance = float('inf')
        
        print(f"    Searching for exact {['', 'rise', 'set'][event_type]} time between {start_time.strftime('%H:%M')} and {end_time.strftime('%H:%M')}...")
        
        while current_time <= end_time:
            position_data = self.calculate_position_at_time(body, current_body_name, current_time)
            altitude = position_data['altitude']
            
            # Find the point closest to horizon (0°)
            distance_to_horizon = abs(altitude - horizon)
            if distance_to_horizon < closest_distance:
                closest_distance = distance_to_horizon
                closest_to_horizon = position_data.copy()
                closest_to_horizon['event'] = event_type
                closest_to_horizon['distance_event'] = DISTANCE_EVENT_NONE
            
            current_time += time_delta
        
        if closest_to_horizon:
            event_name = 'rise' if event_type == EVENT_RISE else 'set'
            print(f"      Found exact {event_name} at {closest_to_horizon['datetime_utc']} (altitude: {closest_to_horizon['altitude']:.3f}°)")
            return closest_to_horizon
        
        return None
    
    def detect_rise_set_events(self, alt_prev, alt_curr):
        """Detect rise/set events only."""
        horizon = 0.0  # degrees
        
        # Rise event: below -> above horizon
        if alt_prev <= horizon and alt_curr > horizon:
            return EVENT_RISE
        
        # Set event: above -> below horizon  
        if alt_prev > horizon and alt_curr <= horizon:
            return EVENT_SET
            
        return EVENT_NONE
    
    def find_local_culminations(self, records, min_altitude_threshold=10.0):
        """
        Find local culmination events (altitude peaks) in the dataset.
        A culmination is detected when altitude is higher than both neighbors
        and above the minimum threshold.
        """
        culminations_found = 0
        
        for i in range(1, len(records) - 1):
            current_alt = records[i]['altitude']
            prev_alt = records[i-1]['altitude']
            next_alt = records[i+1]['altitude']
            
            # Check if this is a local maximum above threshold
            if (current_alt > prev_alt and 
                current_alt > next_alt and 
                current_alt >= min_altitude_threshold):
                
                # Make sure this isn't already marked as an event
                if records[i]['event'] == EVENT_NONE:
                    records[i]['event'] = EVENT_CULMINATION
                    culminations_found += 1
                    print(f"    Culmination at {records[i]['datetime_utc']} with altitude {current_alt:.1f}°")
        
        return culminations_found
    
    def generate_ephemeris(self, celestial_body, start_date, end_date, time_step_seconds=60):
        """
        Generate ephemeris data for a celestial body.
        
        Args:
            celestial_body: 'sun', 'moon', or planet name
            start_date: datetime object (will be converted to UTC if naive)
            end_date: datetime object (will be converted to UTC if naive)
            time_step_seconds: time step in seconds
            
        Returns:
            List of ephemeris records
        """
        # Ensure dates are timezone-aware (UTC)
        if start_date.tzinfo is None:
            start_date = start_date.replace(tzinfo=utc)
        if end_date.tzinfo is None:  
            end_date = end_date.replace(tzinfo=utc)
            
        records = []
        current_time = start_date
        time_delta = timedelta(seconds=time_step_seconds)
        
        # Get the celestial object
        if celestial_body.lower() == 'sun':
            body = self.sun
        elif celestial_body.lower() == 'moon':
            body = self.moon
        else:
            body = self.planets.get(celestial_body.lower())
            if body is None:
                raise ValueError(f"Unknown celestial body: {celestial_body}")
        
        prev_altitude = None
        prev_time = None
        
        print(f"Generating ephemeris from {start_date} to {end_date} with {time_step_seconds}s steps...")
        total_steps = int((end_date - start_date).total_seconds() / (time_step_seconds))
        step_count = 0
        
        while current_time <= end_date:
            if step_count % 100 == 0:  # Progress indicator
                print(f"  Progress: {step_count}/{total_steps} steps ({100*step_count/total_steps:.1f}%)")
            
            # Calculate position data
            position_data = self.calculate_position_at_time(body, celestial_body, current_time)
            altitude_deg = position_data['altitude']
            
            # Detect rise/set events and find exact times
            event = EVENT_NONE
            if prev_altitude is not None:
                event = self.detect_rise_set_events(prev_altitude, altitude_deg)
                
                # # If we detected a rise or set event, find the exact time
                # if event in [EVENT_RISE, EVENT_SET]:
                #     exact_event = self.find_exact_event_time(
                #         body, celestial_body, prev_time, current_time, event, precision_minutes=1
                #     )
                    
                #     if exact_event:
                #         # Insert the exact event record
                #         records.append(exact_event)
            
            # Create regular record
            record = position_data.copy()
            record['event'] = event  # Regular records don't have events (except exact ones)
            record['distance_event'] = DISTANCE_EVENT_NONE
            
            records.append(record)
            prev_altitude = altitude_deg
            prev_time = current_time
            current_time += time_delta
            step_count += 1
            
        print(f"  Phase 1 Complete: Generated {len(records)} records")
        
        # Sort records by timestamp to ensure exact event times are in the right place
        records.sort(key=lambda r: r['timestamp'])
        
        # Post-processing: Find local culminations (altitude peaks)
        print("  Phase 2: Finding culminations...")
        min_altitude = 10.0 if celestial_body.lower() in ['sun', 'moon'] else 0.0
        culminations_found = self.find_local_culminations(records, min_altitude)
        print(f"    Found {culminations_found} culmination events")
        
        # Post-processing: Find apogee and perigee (max and min distances)
        print("  Phase 3: Finding apogee and perigee...")
        min_distance_record = min(records, key=lambda r: r['distance'])
        max_distance_record = max(records, key=lambda r: r['distance'])
        
        min_distance_record['distance_event'] = DISTANCE_EVENT_PERIGEE
        max_distance_record['distance_event'] = DISTANCE_EVENT_APOGEE
        
        print(f"    Perigee found at {min_distance_record['datetime_utc']} with distance {min_distance_record['distance']:.1f} km")
        print(f"    Apogee found at {max_distance_record['datetime_utc']} with distance {max_distance_record['distance']:.1f} km")
        
        print(f"  Complete: Generated {len(records)} records with events")
        return records
    
    def write_csv_file(self, records, filename, celestial_body, start_date, end_date, time_step_hours):
        """Write ephemeris records to CSV file with metadata header."""
        
        with open(filename, 'w', newline='', encoding='utf-8') as csvfile:
            # Write metadata as comments
            csvfile.write(f"# Celestial Ephemeris Data\n")
            csvfile.write(f"# Generated: {datetime.now(utc).strftime('%Y-%m-%d %H:%M:%S')} UTC\n")
            csvfile.write(f"# Celestial Body: {celestial_body}\n")
            csvfile.write(f"# Observer Location: {self.observer_lat:.6f}°, {self.observer_lon:.6f}° (elevation: {self.observer_elevation}m)\n")
            csvfile.write(f"# Time Range: {start_date.strftime('%Y-%m-%d %H:%M:%S')} to {end_date.strftime('%Y-%m-%d %H:%M:%S')} UTC\n")
            csvfile.write(f"# Time Step: {time_step_hours} seconds(s) with precise rise/set events\n")
            csvfile.write(f"# Custom Epoch: January 4, 1992 23:05:37 UTC (offset from Unix: {CUSTOM_EPOCH_OFFSET} seconds)\n")
            csvfile.write(f"# Total Records: {len(records)}\n")
            csvfile.write(f"#\n")
            csvfile.write(f"# Event Codes: 0=None, 1=Rise, 2=Set, 3=Culmination\n")
            csvfile.write(f"# Distance Event Codes: 0=None, 1=Perigee, 2=Apogee\n")
            csvfile.write(f"# Planet IDs: Mercury=1, Venus=2, Mars=4, Jupiter=5, Saturn=6, Uranus=7, Neptune=8, Sun=10, Moon=11\n")
            csvfile.write(f"#\n")
            csvfile.write(f"# Field Descriptions:\n")
            csvfile.write(f"# timestamp: Seconds since custom epoch\n")
            csvfile.write(f"# phase: Moon phase (0.0=new, 1.0=full) or 0.0 for other bodies\n")
            csvfile.write(f"# distance_km: Distance from Earth in kilometers\n")
            csvfile.write(f"# azimuth_deg: Azimuth angle in degrees (0=North, 90=East)\n")
            csvfile.write(f"# altitude_deg: Altitude angle in degrees (0=horizon, 90=zenith)\n")
            csvfile.write(f"# event: Rise/Set/Culmination event type\n")
            csvfile.write(f"# distance_event: Apogee/Perigee event type\n")
            csvfile.write(f"# closest_planet_id: ID of closest planet\n")
            csvfile.write(f"# angular_distance_deg: Angular distance to closest planet in degrees\n")
            csvfile.write(f"# datetime_utc: Human-readable timestamp (for reference)\n")
            csvfile.write(f"# NOTE: Precise rise/set events are inserted with exact crossing times\n")
            csvfile.write(f"# NOTE: Culminations are local maxima above 10° altitude threshold\n")
            csvfile.write(f"#\n")
            
            # Define CSV columns
            fieldnames = [
                'timestamp',
                'phase', 
                'distance_km',
                'azimuth_deg',
                'altitude_deg',
                'event',
                'distance_event',
                'closest_planet_id',
                'angular_distance_deg',
                'datetime_utc'
            ]
            
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            writer.writeheader()
            
            # Write data rows
            for record in records:
                # Format numeric values to reasonable precision
                formatted_record = {
                    'timestamp': record['timestamp'],
                    'phase': f"{record['phase']:.6f}",
                    'distance_km': f"{record['distance']:.1f}",
                    'azimuth_deg': f"{record['azimuth']:.4f}",
                    'altitude_deg': f"{record['altitude']:.4f}",
                    'event': record['event'],
                    'distance_event': record['distance_event'],
                    'closest_planet_id': record['closest_planet_id'],
                    'angular_distance_deg': f"{record['angular_distance']:.4f}",
                    'datetime_utc': record['datetime_utc']
                }
                writer.writerow(formatted_record)
        
        file_size = Path(filename).stat().st_size
        print(f"CSV file written: {filename} ({len(records)} records, {file_size:,} bytes)")
    
    def read_csv_sample(self, filename, num_samples=10):
        """Read and display sample records from CSV file, including precise events."""
        # Read all lines first
        with open(filename, 'r', encoding='utf-8') as file:
            all_lines = file.readlines()
        
        # Filter out comment lines and create clean CSV content
        data_lines = []
        for line in all_lines:
            if not line.strip().startswith('#'):
                data_lines.append(line)
        
        # Create a temporary string to parse as CSV
        csv_content = ''.join(data_lines)
        
        # Parse CSV data
        import io
        csv_file = io.StringIO(csv_content)
        reader = csv.DictReader(csv_file)
        records = list(reader)
        
        print(f"\nSample records from {filename}:")
        print(f"Total records in file: {len(records)}")
        
        # Show sample records including events
        event_records = [r for r in records if int(r['event']) != 0 or int(r['distance_event']) != 0]
        regular_records = [r for r in records if int(r['event']) == 0 and int(r['distance_event']) == 0]
        
        print(f"Records with events: {len(event_records)}")
        print(f"Regular records: {len(regular_records)}")
        
        # Count event types
        rise_count = len([r for r in event_records if int(r['event']) == EVENT_RISE])
        set_count = len([r for r in event_records if int(r['event']) == EVENT_SET])
        culmination_count = len([r for r in event_records if int(r['event']) == EVENT_CULMINATION])
        
        print(f"Rise events: {rise_count}")
        print(f"Set events: {set_count}")
        print(f"Culmination events: {culmination_count}")
        
        # Planet ID to name mapping for display
        planet_names = {v: k.capitalize() for k, v in PLANET_MAP.items()}
        
        # Show event records first
        print("\nEvent Records:")
        for record in event_records[:num_samples//2]:
            event_names = ['None', 'Rise', 'Set', 'Culmination']
            distance_event_names = ['None', 'Perigee', 'Apogee']
            
            event_name = event_names[int(record['event'])] if int(record['event']) < len(event_names) else 'Unknown'
            distance_event_name = distance_event_names[int(record['distance_event'])] if int(record['distance_event']) < len(distance_event_names) else 'Unknown'
            closest_planet_name = planet_names.get(int(record['closest_planet_id']), 'Unknown')
            
            print(f"  ★ {record['datetime_utc']} | "
                  f"Alt: {float(record['altitude_deg']):6.1f}° | "
                  f"Az: {float(record['azimuth_deg']):6.1f}° | "
                  f"Event: {event_name} | "
                  f"Dist.Event: {distance_event_name} | "
                  f"Closest: {closest_planet_name} ({float(record['angular_distance_deg']):.1f}°)")
        
        # Show some regular records
        print("\nRegular Records (sample):")
        indices = [0, len(regular_records)//4, len(regular_records)//2, len(regular_records)*3//4, len(regular_records)-1]
        for i in indices[:num_samples//2]:
            if i < len(regular_records):
                record = regular_records[i]
                closest_planet_name = planet_names.get(int(record['closest_planet_id']), 'Unknown')
                
                print(f"  [{i:3d}] {record['datetime_utc']} | "
                      f"Alt: {float(record['altitude_deg']):6.1f}° | "
                      f"Az: {float(record['azimuth_deg']):6.1f}° | "
                      f"Phase: {float(record['phase']):.3f} | "
                      f"Closest: {closest_planet_name} ({float(record['angular_distance_deg']):.1f}°)")

# Example usage
def main():
    # Configuration
    observer_lat = 12.9822196   
    observer_lon = 46.1406844  
    observer_elevation = 220   # meters
    
    # Create generator
    generator = CelestialEphemerisGenerator(observer_lat, observer_lon, observer_elevation)
    
    # Generate ephemeris for multiple bodies
    celestial_bodies = ['moon']
    start_date = datetime(2025, 6, 9, 0, 0, 0)  # Will be converted to UTC
    end_date = datetime(2025, 7, 9, 0, 0, 0)  # Will be converted to UTC
    time_step = 60  # secs
    
    for body in celestial_bodies:
        print(f"\n{'='*80}")
        print(f"Generating {body.upper()} ephemeris with precise rise/set times...")
        
        # Generate data
        records = generator.generate_ephemeris(body, start_date, end_date, time_step)
        
        # Write to CSV file
        csv_filename = f'{body}_ephemeris_precise_{start_date.strftime("%Y%m%d")}.csv'
        generator.write_csv_file(records, csv_filename, body, start_date, end_date, time_step)
        
        # Show sample data
        generator.read_csv_sample(csv_filename)
    
    print(f"\n{'='*80}")
    print("Generation complete!")
    print("\nFeatures included:")
    print("✓ Precise rise/set times (1-minute accuracy)")
    print("✓ Local culmination detection (daily peaks)")
    print("✓ Apogee/Perigee detection (distance extremes)")
    print("✓ Closest planet tracking")
    print("✓ Moon phase calculation")

if __name__ == "__main__":
    main()
