#ifndef MANDELBROT_INCLUDED
#define MANDELBROT_INCLUDED

int computeMandelbrot(float2 p, int iterations)
{
    float x = 0;
    float y = 0;
    int i = 0;
    while (i < iterations && x * x + y * y <= 4)
    {
        const float x_temp = x * x - y * y + p.x;
        y = 2 * x * y + p.y;
        x = x_temp;
        i++;
    }
    return i;
}


#endif