using UnityEngine;

[System.Serializable]
public class ZernikeMoment
{
    public float magnitude;
    public float phase;

    public ZernikeMoment(float mag, float ph)
    {
        magnitude = mag;
        phase = ph;
    }
}
