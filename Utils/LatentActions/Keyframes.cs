using System.Collections.Generic;

public struct KeyframesFloat
{
    public KeyframesFloat(List<(float time, float value)> inVals)
    {
        vals = inVals;
    }
    public List<(float time, float value)> vals;
}

public static class Keyframes
{
    // Assumes frames are in order
    public static float GetValue(KeyframesFloat frames, float time)
    {
        if (frames.vals.Count == 0)
        {
            return 0;
        }
        else if (frames.vals.Count == 1)
        {
            return frames.vals[0].value;
        }
        else
        {
            if (time < frames.vals[0].time)
            {
                return frames.vals[0].value;
            }
            // track through and find between vals
            for (int i = 1; i < frames.vals.Count; i++)
            {
                // >= and < to make sure no 0 length intervals; prioritizes second value
                if (time >= frames.vals[i-1].time && time < frames.vals[i].time)
                {
                    float interval = frames.vals[i].time - frames.vals[i-1].time;
                    float proportion = (time - frames.vals[i-1].time) / interval;

                    return proportion * frames.vals[i].value + (1 - proportion) * frames.vals[i-1].value;
                }
            }

            return frames.vals[frames.vals.Count - 1].value;
        }
    }

}