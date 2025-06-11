
import csv
import struct
from datetime import datetime, timedelta
from pathlib import Path
import numpy as np
from skyfield.api import load, Topos, utc
from skyfield.almanac import find_discrete, risings_and_settings

# Custom epoch: January 4, 1992, 23:05:37 UTC 694566337
CUSTOM_EPOCH = datetime(1992, 1, 4, 23, 5, 37, tzinfo=utc)
UNIX_EPOCH = datetime(1970, 1, 1, 0, 0, 0, tzinfo=utc)
CUSTOM_EPOCH_OFFSET = int((CUSTOM_EPOCH - UNIX_EPOCH).total_seconds())

# Event types for the event file
EVENT_RISE = 1
EVENT_SET = 2
EVENT_CULMINATION = 3           # Upper culmination (meridian transit)
EVENT_ANTI_CULMINATION = 4      # Lower culmination (altitude minimum)
EVENT_PLANET_TRANSIT = 5        # Closest planet changes
EVENT_APOGEE = 6               # Maximum distance
EVENT_PERIGEE = 7              # Minimum distance
EVENT_NEW_MOON = 8             # Moon phase events (keeping for compatibility)
EVENT_FULL_MOON = 9
EVENT_FIRST_QUARTER = 10
EVENT_LAST_QUARTER = 11
EVENT_CONJUNCTION = 12          # 0° from sun (superior/inferior conjunction)
EVENT_QUADRATURE_EAST = 13      # 90° from sun (eastern quadrature)
EVENT_OPPOSITION = 14           # 180° from sun (opposition)
EVENT_QUADRATURE_WEST = 15      # 270° from sun (western quadrature)

# Planet IDs
PLANET_MAP = {
    'mercury': 1, 'venus': 2, 'mars': 4, 'jupiter': 5, 'saturn': 6,
    'uranus': 7, 'neptune': 8, 'sun': 10, 'moon': 11, 'none': 0
}

# Standard horizon corrections (in degrees)
HORIZON_CORRECTIONS = {
    'sun': -0.8333,      # Standard correction: refraction + semi-diameter
    'moon': -0.8333,     # Same as sun
    'planets': -0.5667,  # Just atmospheric refraction
}

class DualFileEphemerisGenerator:
    def __init__(self, observer_lat, observer_lon, observer_elevation=0):
        """Initialize the ephemeris generator."""
        self.observer = Topos(observer_lat, observer_lon, elevation_m=observer_elevation)
        self.observer_lat = observer_lat
        self.observer_lon = observer_lon
        self.observer_elevation = observer_elevation
        self.ts = load.timescale()
        self.planets = load('de421.bsp')
        
        # Load celestial objects
        self.sun = self.planets['sun']
        self.moon = self.planets['moon']
        self.earth = self.planets['earth']
        self.uranus = self.planets['uranus barycenter']

        # Available planets for transit detection
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
        if dt.tzinfo is None:
            dt = dt.replace(tzinfo=utc)
        unix_timestamp = dt.timestamp()
        return int(unix_timestamp - CUSTOM_EPOCH_OFFSET)
    
    def find_phase_transit_events(self, body, celestial_body, start_date, end_date):
        """Find phase transit events when angular separation from sun crosses key angles."""
        print(f"  Finding phase transit events...")
        
        events = []
        current_time = start_date
        time_delta = timedelta(hours=6)  # Check every 6 hours
        
        # Key angles to detect (in degrees)
        key_angles = [
            (0, EVENT_CONJUNCTION, "Conjunction"),
            (90, EVENT_QUADRATURE_EAST, "Eastern Quadrature"), 
            (180, EVENT_OPPOSITION, "Opposition"),
            (270, EVENT_QUADRATURE_WEST, "Western Quadrature")
        ]
        
        prev_separation = None
        
        while current_time <= end_date:
            t = self.ts.from_datetime(current_time)
            observer_at_t = self.earth + self.observer
            
            # Calculate angular separation from sun
            body_apparent = observer_at_t.at(t).observe(body).apparent()
            sun_apparent = observer_at_t.at(t).observe(self.sun).apparent()
            separation = body_apparent.separation_from(sun_apparent)
            separation_deg = separation.degrees
            
            if prev_separation is not None:
                # Check each key angle for crossings
                for target_angle, event_type, event_name in key_angles:
                    # Check for crossing (considering 360° wrap-around)
                    crossed = False
                    
                    if target_angle == 0:  # Conjunction (0°)
                        # Handle wrap-around at 0°/360°
                        if (prev_separation > 350 and separation_deg < 10) or \
                        (prev_separation > 10 and separation_deg < 10 and prev_separation > separation_deg):
                            crossed = True
                    elif target_angle == 180:  # Opposition
                        if prev_separation < 180 and separation_deg >= 180:
                            crossed = True
                        elif prev_separation > 180 and separation_deg <= 180:
                            crossed = True
                    else:  # Quadratures (90°, 270°)
                        tolerance = 5  # degrees
                        if abs(prev_separation - target_angle) > tolerance and \
                        abs(separation_deg - target_angle) <= tolerance:
                            crossed = True
                    
                    if crossed:
                        # Find more precise crossing time
                        precise_time = self.find_precise_phase_transit(
                            body, celestial_body, current_time - time_delta, current_time, target_angle
                        )
                        
                        if precise_time:
                            stream_data = self.calculate_stream_data(body, celestial_body, precise_time)
                            
                            event = {
                                'timestamp': stream_data['timestamp'],
                                'event_type': event_type,
                                'azimuth_deg': stream_data['azimuth_deg'],
                                'altitude_deg': stream_data['altitude_deg'],
                                'phase': stream_data['phase'],  # This will be the actual angle at crossing
                                'distance_km': stream_data['distance_km']
                            }
                            events.append(event)
                            
                            print(f"    {event_name}: {precise_time.strftime('%Y-%m-%d %H:%M:%S')} (separation: {event['phase']:.1f}°)")
            
            prev_separation = separation_deg
            current_time += time_delta
        
        return events

    def find_precise_phase_transit(self, body, celestial_body, start_time, end_time, target_angle):
        """Find precise time of phase transit crossing."""
        current_time = start_time
        time_delta = timedelta(minutes=30)  # 30-minute precision
        
        closest_time = None
        closest_distance = float('inf')
        
        while current_time <= end_time:
            t = self.ts.from_datetime(current_time)
            observer_at_t = self.earth + self.observer
            
            body_apparent = observer_at_t.at(t).observe(body).apparent()
            sun_apparent = observer_at_t.at(t).observe(self.sun).apparent()
            separation = body_apparent.separation_from(sun_apparent)
            separation_deg = separation.degrees
            
            # Calculate distance from target angle (handling wrap-around)
            if target_angle == 0:
                distance = min(separation_deg, 360 - separation_deg)
            else:
                distance = abs(separation_deg - target_angle)
            
            if distance < closest_distance:
                closest_distance = distance
                closest_time = current_time
            
            current_time += time_delta
        
        return closest_time

    def get_moon_phase(self, t):
        """Calculate moon phase (0.0 = new moon, 1.0 = full moon)."""
        earth = self.planets['earth']
        sun = self.planets['sun']
        moon = self.planets['moon']
        
        earth_pos = earth.at(t)
        sun_apparent = earth_pos.observe(sun)
        moon_apparent = earth_pos.observe(moon)
        
        elongation = sun_apparent.separation_from(moon_apparent)
        elongation_degrees = elongation.degrees
        
        phase = (1.0 - np.cos(np.radians(elongation_degrees))) / 2.0
        return phase
    
    def find_closest_planet(self, target_body, t, current_body_name):
        """Find the closest planet to the target celestial body."""
        earth_pos = self.earth.at(t)
        target_apparent = earth_pos.observe(target_body).apparent()
        
        closest_distance = float('inf')
        closest_planet_id = 0
        
        for planet_name, planet_body in self.available_planets.items():
            if planet_name.lower() == current_body_name.lower():
                continue
                
            try:
                planet_apparent = earth_pos.observe(planet_body).apparent()
                separation = target_apparent.separation_from(planet_apparent)
                separation_degrees = separation.degrees
                
                if separation_degrees < closest_distance:
                    closest_distance = separation_degrees
                    closest_planet_id = PLANET_MAP.get(planet_name, 0)
                    
            except Exception:
                continue
        
        return closest_planet_id, closest_distance
    
    def write_stream_csv(self, stream_records, filename, celestial_body, start_date, end_date, interval_seconds):
        """Write stream data to CSV file for debugging."""
        print(f"\nWriting stream CSV: {filename}")
        
        with open(filename, 'w', newline='', encoding='utf-8') as csvfile:
            writer = csv.writer(csvfile)
            comm = "#"
           
        
            # Write header
            writer.writerow([
                'datetime_utc',
                'timestamp',
                'phase_or_separation', 
                'distance_km',
                'azimuth_deg',
                'altitude_deg'
            ])
            
            # Write stream records
            for record in stream_records:
                dt = datetime.fromtimestamp(record['timestamp'] + CUSTOM_EPOCH_OFFSET, tz=utc)
                writer.writerow([
                    dt.strftime('%Y-%m-%d %H:%M:%S'),
                    record['timestamp'],
                    f"{record['phase']:.6f}",
                    f"{record['distance_km']:.1f}",
                    f"{record['azimuth_deg']:.4f}",
                    f"{record['altitude_deg']:.4f}"
                ])
        
        file_size = Path(filename).stat().st_size
        print(f"  Written: {len(stream_records)} stream records, {file_size:,} bytes")

    

    def write_events_csv(self, events, filename, celestial_body):
        """Write events data to CSV file for debugging."""
        print(f"\nWriting events CSV: {filename}")
        
        with open(filename, 'w', newline='', encoding='utf-8') as csvfile:
            writer = csv.writer(csvfile)
            
            # Write header directly
            writer.writerow([
                'datetime_utc',
                'timestamp',
                'event_type',
                'event_name',
                'phase_or_separation',
                'distance_km', 
                'azimuth_deg',
                'altitude_deg',
                'from_planet',
                'to_planet'
            ])
            
            # Event type names
            event_names = {
                EVENT_RISE: 'Rise',
                EVENT_SET: 'Set', 
                EVENT_CULMINATION: 'Culmination',
                EVENT_ANTI_CULMINATION: 'Anti-culmination',
                EVENT_PLANET_TRANSIT: 'Planet Transit',
                EVENT_APOGEE: 'Apogee',
                EVENT_PERIGEE: 'Perigee',
                EVENT_NEW_MOON: 'New Moon',
                EVENT_FULL_MOON: 'Full Moon',
                EVENT_FIRST_QUARTER: 'First Quarter',
                EVENT_LAST_QUARTER: 'Last Quarter',
                EVENT_CONJUNCTION: 'Conjunction',
                EVENT_QUADRATURE_EAST: 'Eastern Quadrature',
                EVENT_OPPOSITION: 'Opposition',
                EVENT_QUADRATURE_WEST: 'Western Quadrature'
            }
            
            # Planet ID to name mapping
            planet_id_to_name = {v: k.capitalize() for k, v in PLANET_MAP.items()}
            
            # Write event records
            for event in events:
                dt = datetime.fromtimestamp(event['timestamp'] + CUSTOM_EPOCH_OFFSET, tz=utc)
                event_name = event_names.get(event['event_type'], f"Unknown_{event['event_type']}")
                
                # Handle planet transit details
                from_planet = ''
                to_planet = ''
                if event['event_type'] == EVENT_PLANET_TRANSIT:
                    from_planet = planet_id_to_name.get(event.get('from_planet_id', 0), '')
                    to_planet = planet_id_to_name.get(event.get('to_planet_id', 0), '')
                
                writer.writerow([
                    dt.strftime('%Y-%m-%d %H:%M:%S'),
                    event['timestamp'],
                    event['event_type'],
                    event_name,
                    f"{event['phase']:.6f}",
                    f"{event['distance_km']:.1f}",
                    f"{event['azimuth_deg']:.4f}",
                    f"{event['altitude_deg']:.4f}",
                    from_planet,
                    to_planet
                ])
        
        file_size = Path(filename).stat().st_size
        print(f"  Written: {len(events)} event records, {file_size:,} bytes")

    def calculate_stream_data(self, body, current_body_name, time_dt):
        """Calculate streamlined data for the ephemeral stream file."""
        t = self.ts.from_datetime(time_dt)
        observer_at_t = self.earth + self.observer
        apparent = observer_at_t.at(t).observe(body).apparent()
        
        ra, dec, distance = apparent.radec()
        alt, az, d = apparent.altaz()
        
        timestamp = self.datetime_to_custom_epoch(time_dt)
        
        # Calculate phase (for moon) or angular separation from sun (for other bodies)
        if current_body_name.lower() == 'moon':
            phase = self.get_moon_phase(t)
        else:
            # Calculate angular separation from sun for planets
            sun_apparent = observer_at_t.at(t).observe(self.sun).apparent()
            separation = apparent.separation_from(sun_apparent)
            phase = separation.degrees  # Store angular separation in degrees
        
        return {
            'timestamp': timestamp,
            'phase': phase,
            'distance_km': distance.km,
            'azimuth_deg': az.degrees,
            'altitude_deg': alt.degrees
        }
    
    def get_horizon_correction(self, celestial_body):
        """Get the appropriate horizon correction for the celestial body."""
        body_name = celestial_body.lower()
        if body_name in ['sun', 'moon']:
            return HORIZON_CORRECTIONS[body_name]
        else:
            return HORIZON_CORRECTIONS['planets']
    
    def find_rise_set_events(self, body, celestial_body, start_date, end_date):
        """Find precise rise and set events."""
        print(f"  Finding rise/set events...")
        
        horizon_degrees = self.get_horizon_correction(celestial_body)
        t0 = self.ts.from_datetime(start_date)
        t1 = self.ts.from_datetime(end_date)
        observer_location = self.earth + self.observer
        
        def body_above_horizon(t):
            apparent = observer_location.at(t).observe(body).apparent()
            alt, az, d = apparent.altaz()
            return alt.degrees > horizon_degrees
        body_above_horizon.step_days = 0.1
        times, is_rising = find_discrete(t0, t1, body_above_horizon)
        
        events = []
        for time, rising in zip(times, is_rising):
            event_dt = time.utc_datetime()
            
            # Calculate position at event time
            stream_data = self.calculate_stream_data(body, celestial_body, event_dt)
            
            event = {
                'timestamp': stream_data['timestamp'],
                'event_type': EVENT_RISE if rising else EVENT_SET,
                'azimuth_deg': stream_data['azimuth_deg'],
                'altitude_deg': stream_data['altitude_deg'],
                'phase': stream_data['phase'],
                'distance_km': stream_data['distance_km']
            }
            events.append(event)
            
            event_name = "rise" if rising else "set"
            print(f"    {event_name.capitalize()}: {event_dt.strftime('%Y-%m-%d %H:%M:%S')} (alt: {event['altitude_deg']:.3f}°)")
        
        return events
    
    def find_culmination_events(self, body, celestial_body, start_date, end_date):
        """Find culmination and anti-culmination events."""
        print(f"  Finding culmination events...")
        
        t0 = self.ts.from_datetime(start_date)
        t1 = self.ts.from_datetime(end_date)
        observer_location = self.earth + self.observer
        
        def body_hour_angle(t):
            apparent = observer_location.at(t).observe(body).apparent()
            ra, dec, distance = apparent.radec()
            
            lst = t.gast + (self.observer_lon / 15.0)
            hour_angle = lst - ra.hours
            
            while all(hour_angle > 12):
                hour_angle -= 24
            while all(hour_angle < -12):
                hour_angle += 24
                
            return hour_angle < 0
        

        body_hour_angle.step_days = 0.1

        times, crossing_upward = find_discrete(t0, t1, body_hour_angle)
        
        events = []
        for time, upward in zip(times, crossing_upward):
            event_dt = time.utc_datetime()
            stream_data = self.calculate_stream_data(body, celestial_body, event_dt)
            
            # Determine if this is upper or lower culmination
            if stream_data['altitude_deg'] > 0:
                event_type = EVENT_CULMINATION
                event_name = "culmination"
            else:
                event_type = EVENT_ANTI_CULMINATION  
                event_name = "anti-culmination"
            
            event = {
                'timestamp': stream_data['timestamp'],
                'event_type': event_type,
                'azimuth_deg': stream_data['azimuth_deg'],
                'altitude_deg': stream_data['altitude_deg'],
                'phase': stream_data['phase'],
                'distance_km': stream_data['distance_km']
            }
            events.append(event)
            
            print(f"    {event_name.capitalize()}: {event_dt.strftime('%Y-%m-%d %H:%M:%S')} (alt: {event['altitude_deg']:.1f}°)")
        
        return events
    
    def find_planet_transit_events(self, body, celestial_body, start_date, end_date, check_interval_minutes=30):
        """Find when the closest planet changes."""
        print(f"  Finding planet transit events...")
        
        events = []
        current_time = start_date
        time_delta = timedelta(minutes=check_interval_minutes)
        prev_closest_id = None
        
        while current_time <= end_date:
            t = self.ts.from_datetime(current_time)
            closest_id, _ = self.find_closest_planet(body, t, celestial_body)
            
            if prev_closest_id is not None and closest_id != prev_closest_id:
                # Planet transit detected - find more precise time
                precise_time = self.find_precise_planet_transit(
                    body, celestial_body, current_time - time_delta, current_time, 
                    prev_closest_id, closest_id
                )
                
                if precise_time:
                    stream_data = self.calculate_stream_data(body, celestial_body, precise_time)
                    
                    event = {
                        'timestamp': stream_data['timestamp'],
                        'event_type': EVENT_PLANET_TRANSIT,
                        'azimuth_deg': stream_data['azimuth_deg'],
                        'altitude_deg': stream_data['altitude_deg'],
                        'phase': stream_data['phase'],
                        'distance_km': stream_data['distance_km'],
                        'from_planet_id': prev_closest_id,
                        'to_planet_id': closest_id
                    }
                    events.append(event)
                    
                    from_name = [k for k, v in PLANET_MAP.items() if v == prev_closest_id][0] if prev_closest_id in PLANET_MAP.values() else 'Unknown'
                    to_name = [k for k, v in PLANET_MAP.items() if v == closest_id][0] if closest_id in PLANET_MAP.values() else 'Unknown'
                    print(f"    Transit: {precise_time.strftime('%Y-%m-%d %H:%M:%S')} ({from_name} → {to_name})")
            
            prev_closest_id = closest_id
            current_time += time_delta
        
        return events
    
    def find_precise_planet_transit(self, body, celestial_body, start_time, end_time, from_id, to_id):
        """Find precise time of planet transit using binary search."""
        current_time = start_time
        time_delta = timedelta(minutes=1)
        
        while current_time <= end_time:
            t = self.ts.from_datetime(current_time)
            closest_id, _ = self.find_closest_planet(body, t, celestial_body)
            
            if closest_id == to_id:
                return current_time
            
            current_time += time_delta
        
        return None
    
    def find_moon_phase_events(self, start_date, end_date):
        """Find major moon phase events."""
        if hasattr(self, 'current_body') and self.current_body.lower() != 'moon':
            return []
            
        print(f"  Finding moon phase events...")
        
        events = []
        current_time = start_date
        time_delta = timedelta(hours=2)  # Reduced to 2 hours for better detection
        
        # Store phase history for peak/trough detection
        phase_history = []
        time_history = []
        
        while current_time <= end_date:
            t = self.ts.from_datetime(current_time)
            phase = self.get_moon_phase(t)
            
            phase_history.append(phase)
            time_history.append(current_time)
            
            # Keep only last 5 points for local extrema detection
            if len(phase_history) > 5:
                phase_history.pop(0)
                time_history.pop(0)
            
            # Need at least 3 points to detect extrema
            if len(phase_history) >= 3:
                # Check if middle point is a local maximum (full moon) or minimum (new moon)
                mid_idx = len(phase_history) // 2
                mid_phase = phase_history[mid_idx]
                mid_time = time_history[mid_idx]
                
                # Check for local maximum (full moon)
                if (mid_idx > 0 and mid_idx < len(phase_history) - 1):
                    is_max = (mid_phase > phase_history[mid_idx - 1] and 
                            mid_phase > phase_history[mid_idx + 1] and
                            mid_phase > 0.85)  # Must be reasonably close to full
                    
                    is_min = (mid_phase < phase_history[mid_idx - 1] and 
                            mid_phase < phase_history[mid_idx + 1] and
                            mid_phase < 0.15)  # Must be reasonably close to new
                    
                    # Check for quarter phases (around 0.5)
                    is_quarter = (abs(mid_phase - 0.5) < 0.1)
                    
                    if is_max:
                        # Full moon detected
                        precise_time = self.find_precise_moon_phase(mid_time, 1.0, timedelta(hours=4))
                        if precise_time:
                            self.add_moon_phase_event(events, precise_time, EVENT_FULL_MOON, "Full Moon")
                    
                    elif is_min:
                        # New moon detected  
                        precise_time = self.find_precise_moon_phase(mid_time, 0.0, timedelta(hours=4))
                        if precise_time:
                            self.add_moon_phase_event(events, precise_time, EVENT_NEW_MOON, "New Moon")
                    
                    elif is_quarter:
                        # Determine if first or last quarter by checking trend
                        if len(phase_history) >= 3:
                            trend = phase_history[-1] - phase_history[0]  # Overall trend
                            if trend > 0:
                                # Phase increasing -> First quarter
                                precise_time = self.find_precise_moon_phase(mid_time, 0.5, timedelta(hours=4))
                                if precise_time:
                                    self.add_moon_phase_event(events, precise_time, EVENT_FIRST_QUARTER, "First Quarter")
                            else:
                                # Phase decreasing -> Last quarter
                                precise_time = self.find_precise_moon_phase(mid_time, 0.5, timedelta(hours=4))
                                if precise_time:
                                    self.add_moon_phase_event(events, precise_time, EVENT_LAST_QUARTER, "Last Quarter")
            
            current_time += time_delta
    
        return events

    def find_precise_moon_phase(self, approximate_time, target_phase, search_window):
        """Find precise time when moon phase is closest to target value."""
        start_time = approximate_time - search_window
        end_time = approximate_time + search_window
        
        best_time = approximate_time
        best_distance = float('inf')
        
        current_time = start_time
        time_step = timedelta(minutes=30)
        
        while current_time <= end_time:
            t = self.ts.from_datetime(current_time)
            phase = self.get_moon_phase(t)
            
            distance = abs(phase - target_phase)
            
            if distance < best_distance:
                best_distance = distance
                best_time = current_time
            
            current_time += time_step
        
        return best_time

    def add_moon_phase_event(self, events, event_time, event_type, event_name):
        """Add a moon phase event to the events list."""
        # Check if we already have this event (avoid duplicates)
        for existing_event in events:
            if (existing_event['event_type'] == event_type and 
                abs(existing_event['timestamp'] - self.datetime_to_custom_epoch(event_time)) < 3600):  # Within 1 hour
                return  # Skip duplicate
        
        stream_data = self.calculate_stream_data(self.moon, 'moon', event_time)
        
        event = {
            'timestamp': stream_data['timestamp'],
            'event_type': event_type,
            'azimuth_deg': stream_data['azimuth_deg'],
            'altitude_deg': stream_data['altitude_deg'],
            'phase': stream_data['phase'],
            'distance_km': stream_data['distance_km']
        }
        events.append(event)
        
        print(f"    {event_name}: {event_time.strftime('%Y-%m-%d %H:%M:%S')} (phase: {event['phase']:.3f})")
    
    def generate_dual_files(self, celestial_body, start_date, end_date, stream_interval_seconds=60):
        """Generate both stream and event files."""
        # Ensure dates are timezone-aware (UTC)
        if start_date.tzinfo is None:
            start_date = start_date.replace(tzinfo=utc)
        if end_date.tzinfo is None:  
            end_date = end_date.replace(tzinfo=utc)
        
        body = self.available_planets[celestial_body.lower()]
        if body is None:
            raise ValueError(f"Unknown celestial body: {celestial_body}")
            
        
        self.current_body = celestial_body  # For moon phase events
        
        print(f"Generating dual files for {celestial_body.upper()}")
        print(f"Date range: {start_date} to {end_date}")
        print(f"Stream interval: {stream_interval_seconds} seconds")
        
        # Phase 1: Generate ephemeral stream data
        print("\n=== GENERATING EPHEMERAL STREAM ===")
        stream_records = []
        current_time = start_date
        time_delta = timedelta(seconds=stream_interval_seconds)
        
        total_steps = int((end_date - start_date).total_seconds() / stream_interval_seconds)
        step_count = 0
        
        while current_time <= end_date:
            if step_count % 1000 == 0:
                print(f"  Progress: {step_count}/{total_steps} ({100*step_count/total_steps:.1f}%)")
            
            stream_data = self.calculate_stream_data(body, celestial_body, current_time)
            stream_records.append(stream_data)
            
            current_time += time_delta
            step_count += 1
        
        print(f"  Generated {len(stream_records)} stream records")
        
        # Phase 2: Generate event data
        print("\n=== GENERATING EVENTS ===")
        all_events = []
        
        # Find different types of events
        rise_set_events = self.find_rise_set_events(body, celestial_body, start_date, end_date)
        all_events.extend(rise_set_events)
        
        culmination_events = self.find_culmination_events(body, celestial_body, start_date, end_date)
        all_events.extend(culmination_events)
        
        transit_events = self.find_planet_transit_events(body, celestial_body, start_date, end_date)
        all_events.extend(transit_events)

        phase_transit_events = self.find_phase_transit_events(body, celestial_body, start_date, end_date)
        all_events.extend(phase_transit_events)
        
        if celestial_body.lower() == 'moon':
            phase_events = self.find_moon_phase_events(start_date, end_date)
            all_events.extend(phase_events)
        
        # Find distance extremes
        if len(stream_records) > 0:
            min_distance_record = min(stream_records, key=lambda r: r['distance_km'])
            max_distance_record = max(stream_records, key=lambda r: r['distance_km'])
            
            perigee_event = {
                'timestamp': min_distance_record['timestamp'],
                'event_type': EVENT_PERIGEE,
                'azimuth_deg': 0.0,  # Not meaningful for distance events
                'altitude_deg': 0.0,
                'phase': min_distance_record['phase'],
                'distance_km': min_distance_record['distance_km']
            }
            
            apogee_event = {
                'timestamp': max_distance_record['timestamp'],
                'event_type': EVENT_APOGEE,
                'azimuth_deg': 0.0,
                'altitude_deg': 0.0, 
                'phase': max_distance_record['phase'],
                'distance_km': max_distance_record['distance_km']
            }
            
            all_events.extend([perigee_event, apogee_event])
            print(f"  Added distance events: perigee ({min_distance_record['distance_km']:.1f} km), apogee ({max_distance_record['distance_km']:.1f} km)")
        
        # Sort events by timestamp
        all_events.sort(key=lambda e: e['timestamp'])
        
        print(f"  Total events: {len(all_events)}")
        
        return stream_records, all_events
    
    def write_binary_stream(self, stream_records, filename, celestial_body, start_date, end_date, interval_seconds):
        """Write stream data to binary file for deterministic access."""
        print(f"\nWriting binary stream: {filename}")
        
        # Binary format: each record is exactly 20 bytes
        # timestamp (4 bytes) + phase (4 bytes) + distance (4 bytes) + azimuth (4 bytes) + altitude (4 bytes)
        record_size = 20
        
        with open(filename, 'wb') as f:
            # Write header (64 bytes total)
            header = struct.pack(
                '<4sIIIIfffffff',
                b'EPHS',  # Magic number (4 bytes)
                len(stream_records),  # Number of records (4 bytes) 
                stream_records[0]['timestamp'] if stream_records else 0,  # Start timestamp (4 bytes)
                stream_records[-1]['timestamp'] if stream_records else 0,  # End timestamp (4 bytes)
                interval_seconds,  # Interval in seconds (4 bytes)
                self.observer_lat,  # Observer latitude (4 bytes)
                self.observer_lon,  # Observer longitude (4 bytes)
                self.observer_elevation,  # Observer elevation (4 bytes)
                0.0, 0.0, 0.0, 0.0  # Reserved space (16 bytes)
            )
            f.write(header)
            
            # Write records
            for record in stream_records:
                record_bytes = struct.pack(
                    '<Iffff',
                    record['timestamp'],
                    record['phase'],
                    record['distance_km'],
                    record['azimuth_deg'],
                    record['altitude_deg']
                )
                f.write(record_bytes)
        
        file_size = Path(filename).stat().st_size
        print(f"  Written: {len(stream_records)} records, {file_size:,} bytes")
        print(f"  Record size: {record_size} bytes (deterministic access)")
        print(f"  Header size: 64 bytes")
        
    def write_binary_events(self, events, filename, celestial_body):
        """Write events to binary file."""
        print(f"\nWriting binary events: {filename}")
        
        # Binary format: each event is exactly 32 bytes
        # timestamp (4) + event_type (4) + azimuth (4) + altitude (4) + phase (4) + distance (4) + extra1 (4) + extra2 (4)
        record_size = 32
        
        with open(filename, 'wb') as f:
            # Write header (64 bytes)
            header = struct.pack(
                '<4sIIIIffffffff',
                b'EVTS',  # Magic number (4 bytes)
                len(events),  # Number of events (4 bytes)
                events[0]['timestamp'] if events else 0,  # First event timestamp (4 bytes)
                events[-1]['timestamp'] if events else 0,  # Last event timestamp (4 bytes)
                0,  # Reserved (4 bytes)
                self.observer_lat,  # Observer latitude (4 bytes)
                self.observer_lon,  # Observer longitude (4 bytes)  
                self.observer_elevation,  # Observer elevation (4 bytes)
                0.0, 0.0, 0.0, 0.0, 0.0  # Reserved space (20 bytes)
            )
            f.write(header)
            
            # Write events
            for event in events:
                # Handle optional fields for planet transits
                extra1 = event.get('from_planet_id', 0)
                extra2 = event.get('to_planet_id', 0)
                
                event_bytes = struct.pack(
                    '<IIffffII',
                    event['timestamp'],
                    event['event_type'],
                    event['azimuth_deg'],
                    event['altitude_deg'],
                    event['phase'],
                    event['distance_km'],
                    extra1,
                    extra2
                )
                f.write(event_bytes)
        
        file_size = Path(filename).stat().st_size
        print(f"  Written: {len(events)} events, {file_size:,} bytes")
        print(f"  Event size: {record_size} bytes (deterministic access)")
        
    def analyze_files(self, stream_filename, events_filename):
        """Analyze the generated files."""
        print(f"\n=== FILE ANALYSIS ===")
        
        # Analyze stream file
        stream_size = Path(stream_filename).stat().st_size
        print(f"Stream file: {stream_filename}")
        print(f"  Size: {stream_size:,} bytes")
        print(f"  Header: 64 bytes")
        print(f"  Data: {stream_size - 64:,} bytes")
        print(f"  Records: {(stream_size - 64) // 20}")
        print(f"  Arduino access: byte_offset = 64 + (minute_index * 20)")
        
        # Analyze events file  
        events_size = Path(events_filename).stat().st_size
        print(f"\nEvents file: {events_filename}")
        print(f"  Size: {events_size:,} bytes")
        print(f"  Header: 64 bytes")
        print(f"  Data: {events_size - 64:,} bytes")
        print(f"  Events: {(events_size - 64) // 32}")
        print(f"  Arduino access: binary search by timestamp")

# Example usage
def main():
    # Configuration
    observer_lat = 52.9822196   
    observer_lon = 36.1406844  
    observer_elevation = 220   # meters
    
    # Create generator
    generator = DualFileEphemerisGenerator(observer_lat, observer_lon, observer_elevation)
    
    # Generate data
    celestial_body = 'moon'
    start_date = datetime(2025, 6, 10, 0, 0, 0)
    end_date = datetime(2025, 7, 12, 0, 0, 0)  # One week
    stream_interval = 60  # 1 minute
    
    print(f"{'='*80}")
    print(f"DUAL-FILE EPHEMERIS GENERATOR")
    print(f"{'='*80}")
    
    # Generate both datasets
    stream_records, events = generator.generate_dual_files(
        celestial_body, start_date, end_date, stream_interval
    )
    
    # Write binary files
    stream_filename = f'{celestial_body}_stream_{start_date.strftime("%Y%m%d")}.bin'
    events_filename = f'{celestial_body}_events_{start_date.strftime("%Y%m%d")}.bin'
    base_name = f'{celestial_body}_{start_date.strftime("%Y%m%d")}'

    generator.write_binary_stream(stream_records, stream_filename, celestial_body, start_date, end_date, stream_interval)
    generator.write_binary_events(events, events_filename, celestial_body)
    generator.write_stream_csv(stream_records, f'{base_name}_stream.csv', celestial_body, start_date, end_date, stream_interval)
    generator.write_events_csv(events, f'{base_name}_events.csv', celestial_body)

    # Analyze results
    generator.analyze_files(stream_filename, events_filename)

if __name__ == "__main__":
    main()
