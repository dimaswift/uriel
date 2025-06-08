from skyfield.api import Loader, Topos
from skyfield import almanac
from datetime import datetime, timedelta, timezone
import numpy as np
import pandas as pd
from astropy.time import Time

load = Loader('~/.skyfield-data')
eph = load('de421.bsp')
ts = load.timescale()

# CONFIG
LAT, LON = 52.9823, 36.1408        
TZ = 'Russia/Moscow'
START = datetime(2025, 6, 1)
END = datetime(2025, 7, 1)
STEP_HOURS = 1

LUNAR_EPOCH = 694566337  # Jan 4, 1992 23:05:37 UTC (solar eclipse)
LUNAR_CYCLE_SECONDS = 2551443

def moon_phase(jd):
    """Returns the phase of the Moon as a fraction (0=new, 0.5=full, 1=new again)"""
    epoch = Time(LUNAR_EPOCH, format='unix').jd
    cycle_frac = ((jd - epoch) % (LUNAR_CYCLE_SECONDS/86400)) / (LUNAR_CYCLE_SECONDS/86400)
    return cycle_frac

def normalize(val, minv, maxv):
    return (val - minv) / (maxv - minv)

def main():
    times = []
    timestamps = []
    phases_norm, phases_actual = [], []
    moon_dist_norm, moon_dist_actual = [], []
    moonrise, moonset, culmination = [], [], []
    
    t0 = START.replace(tzinfo=timezone.utc)
    t1 = END.replace(tzinfo=timezone.utc)
    steps = int((t1 - t0).total_seconds() // (STEP_HOURS * 3600))
    observer = Topos(latitude_degrees=LAT, longitude_degrees=LON)
    min_dist, max_dist = 356500, 406700  # km, rough perigee/apogee

    for i in range(steps):
        t = t0 + timedelta(hours=i)
        ts_t = ts.utc(t.year, t.month, t.day, t.hour, t.minute)
        timestamps.append(int(t.timestamp()))
        times.append(t)
        
        # Moon distance
        astrometric = eph['moon'].at(ts_t).observe(eph['earth']).apparent()
        dist = astrometric.distance().km
        moon_dist_actual.append(dist)
        moon_dist_norm.append(normalize(dist, min_dist, max_dist))
        
        # Moon phase
        jd = ts_t.tt
        phase = moon_phase(jd)
        phases_actual.append(phase)
        phases_norm.append(phase)  # phase is already 0-1, but you can re-normalize if desired
       
        t_start = ts.utc(t.year, t.month, t.day)
        t_end = ts.utc(t.year, t.month, t.day+1)
        f = almanac.risings_and_settings(eph, eph['moon'], observer)
        times_events, events = almanac.find_discrete(t_start, t_end, f)
        
        # 0 is rising, 1 is setting
        rise, set_ = None, None
        for ti, ev in zip(times_events, events):
            if ev == 1 and rise is None:
                rise = ti.utc_datetime()
            elif ev == 0 and set_ is None:
                set_ = ti.utc_datetime()
        if rise: rise = int(rise.timestamp())
        if set_: set_ = int(set_.timestamp())

        if rise and set_: 
            culmination_time = rise + (set_ - rise) / 2 
            culmination.append(culmination_time) 
        else: 
            culmination.append(None) 

        moonrise.append(rise)
        moonset.append(set_)
    
    df = pd.DataFrame({
        'unix_time': timestamps,
        'moon_phase': phases_actual,
        'moon_phase_norm': phases_norm,
        'moon_dist': moon_dist_actual,
        'moon_dist_norm': moon_dist_norm,
        'moon_rise': moonrise,
        'moon_set': moonset,
        'moon_culmination': culmination
    })
    print(df.head())
    df.to_csv('moon_table.csv')
    
if __name__ == '__main__':
    main()