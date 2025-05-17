#ifndef SKY_INCLUDED
#define SKY_INCLUDED

#define PI 3.14159265359
#define PHI 1.618033988749895

struct Star
{
    float frequency;
    float amplitude;
    float phase;
    float dutyCycle;
    float velocity;
    float time;
    float3 location;
};

#endif