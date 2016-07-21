using UnityEngine;
using System.Collections;

//Different wavetypes
public class WaveTypes
{
    private static float frac (float x)
    {
        return x - Mathf.Floor(x);
    }

    private static Vector2 vectorLerp (Vector2 a, Vector2 b, float t)
    {
        Vector2 c = a;

        c.x = (a.x * (1.0f - t)) + (b.x * t);
        c.y = (a.y * (1.0f - t)) + (b.y * t);

        return c;
    }

    private static float rand (Vector2 c)
    {   
        return frac(Mathf.Sin(Vector2.Dot(c, new Vector2(12.9898f, 78.233f))) * 43758.5453f);
    }

    private static Vector4 mod289 (Vector4 x)
    {
        //return x - Mathf.Floor(x * (1.0f / 289.0f)) * 289.0f;

        Vector4 v = new Vector4();
        float c = (1.0f / 289.0f);
        v.x = Mathf.Floor(x.x * c);
        v.y = Mathf.Floor(x.y * c);
        v.z = Mathf.Floor(x.z * c);

        return (x - v * 289.0f);
    }

    private static Vector4 permute (Vector4 x)
    {
        //return mod289(((x * 34.0f) + 1.0f) * x);

        Vector4 v = x * 34.0f;
        v.x += 1.0f;
        v.y += 1.0f;
        v.z += 1.0f;
        v.x *= x.x;
        v.y *= x.y;
        v.z *= x.z;

        return mod289(v);
    }

    private static Vector4 taylorInvSqrt (Vector4 r)
    {
        return (1.79284291400159f - 0.85373472095314f) * r;
    }

    private static Vector2 fade (Vector2 t)
    {
        //return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);

        Vector2 v = t;
        v *= 6.0f;        
        v.x -= 15.0f;
        v.y -= 15.0f;
        v.x *= t.x;
        v.y *= t.y;
        v.x += 10.0f;
        v.y += 10.0f;
        v.x *= t.x;
        v.y *= t.y;
        v.x *= t.x;
        v.y *= t.y;
        v.x *= t.x;
        v.y *= t.y;

        return v;
    }

    // Classic Perlin noise
    private static float cnoise (Vector2 P)
    {
        //Vector4 Pi = floor(P.xyxy) + new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        Vector4 Pi = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        Pi.x += Mathf.Floor(P.x);
        Pi.y += Mathf.Floor(P.y);
        Pi.z += Mathf.Floor(P.x);
        Pi.w += Mathf.Floor(P.y);

        //Vector4 Pf = frac(P.xyxy) - new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        Vector4 Pf = new Vector4(0.0f, 0.0f, -1.0f, -1.0f);
        Pf.x += frac(P.x);
        Pf.y += frac(P.y);
        Pf.z += frac(P.x);
        Pf.w += frac(P.y);

        Pi = mod289(Pi); // To avoid truncation effects in permutation
        //Vector4 ix = Pi.xzxz;
        Vector4 ix = new Vector4(Pi.x, Pi.z, Pi.x, Pi.z);

        //Vector4 iy = Pi.yyww;
        Vector4 iy = new Vector4(Pi.y, Pi.y, Pi.w, Pi.w);

        //Vector4 fx = Pf.xzxz;
        Vector4 fx = new Vector4(Pf.x, Pf.z, Pf.x, Pf.z);

        //Vector4 fy = Pf.yyww;
        Vector4 fy = new Vector4(Pf.y, Pf.y, Pf.w, Pf.w);

        Vector4 i = permute(permute(ix) + iy);

        //Vector4 gx = frac(i * (1.0f / 41.0f)) * 2.0f - 1.0f;
        Vector4 gx = i;
        gx.x *= (1.0f / 41.0f);
        gx.y *= (1.0f / 41.0f);
        gx.z *= (1.0f / 41.0f);
        gx.w *= (1.0f / 41.0f);

        gx.x = frac(gx.x);
        gx.y = frac(gx.y);
        gx.z = frac(gx.z);
        gx.w = frac(gx.w);

        gx *= 2.0f;

        gx.x -= 1.0f;
        gx.y -= 1.0f;
        gx.z -= 1.0f;
        gx.w -= 1.0f;

        //Vector4 gy = Mathf.Abs(gx) - 0.5;
        Vector4 gy = new Vector4(Mathf.Abs(gx.x) - 0.5f, Mathf.Abs(gx.y) - 0.5f, Mathf.Abs(gx.z) - 0.5f, Mathf.Abs(gx.w) - 0.5f);

        //Vector4 tx = floor(gx + 0.5f);
        Vector4 tx = new Vector4(gx.x + 0.5f, gx.y + 0.5f, gx.z + 0.5f, gx.w + 0.5f);

        gx = gx - tx;

        Vector2 g00 = new Vector2(gx.x, gy.x);
        Vector2 g10 = new Vector2(gx.y, gy.y);
        Vector2 g01 = new Vector2(gx.z, gy.z);
        Vector2 g11 = new Vector2(gx.w, gy.w);

        Vector4 norm = taylorInvSqrt(new Vector4(Vector2.Dot(g00, g00), Vector2.Dot(g01, g01), Vector2.Dot(g10, g10), Vector2.Dot(g11, g11)));
        g00 *= norm.x;
        g01 *= norm.y;
        g10 *= norm.z;
        g11 *= norm.w;

        float n00 = Vector2.Dot(g00, new Vector2(fx.x, fy.x));
        float n10 = Vector2.Dot(g10, new Vector2(fx.y, fy.y));
        float n01 = Vector2.Dot(g01, new Vector2(fx.z, fy.z));
        float n11 = Vector2.Dot(g11, new Vector2(fx.w, fy.w));

        //Vector2 fade_xy = fade(Pf.xy);
        Vector2 fade_xy = fade(new Vector2(Pf.x, Pf.y));

        //Vector2 n_x = Mathf.Lerp(new Vector2(n00, n01), new Vector2(n10, n11), fade_xy.x);
        Vector2 n_x = vectorLerp(new Vector2(n00, n01), new Vector2(n10, n11), fade_xy.x);

        float n_xy = Mathf.Lerp(n_x.x, n_x.y, fade_xy.y);
        return 2.3f * n_xy;
    }

    //Sinus waves
    public static float SinXWave (Vector3 position, float speed, float scale, float waveDistance, float noiseStrength, float noiseWalk, float timeSinceStart)
    {
        float x = position.x;
        float y = 0f;
        float z = position.z;

        //Using only x or z will produce straight waves
        //Using only y will produce an up/down movement
        //x + y + z rolling waves
        //x * z produces a moving sea without rolling waves

        float waveType = z;

        y += Mathf.Sin((timeSinceStart * speed + waveType) / waveDistance) * scale;

        //Add noise to make it more realistic
        //y += Mathf.PerlinNoise(x + noiseWalk, y + Mathf.Sin(timeSinceStart * 0.1f)) * noiseStrength;

        y += cnoise(new Vector2(position.x, position.y) * 30.0f + new Vector2(timeSinceStart * 0.01f, timeSinceStart * 0.01f * Mathf.Cos(timeSinceStart * 0.05f)) * noiseWalk) * noiseStrength;

        return y;
    }
}