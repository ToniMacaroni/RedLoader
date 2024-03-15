using FMODCustom;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace SonsSdk;

public class SoundPlayer : MonoBehaviour
{
    public Sound Sound;

    private Channel _channel;
    private Transform t;
    
    public Channel Channel => _channel;

    public (float min, float max) ChannelDistance
    {
        get
        {
            if (_channel == null)
                return (0, 0);
            
            float min, max;
            _channel.get3DMinMaxDistance(out min, out max);
            return (min, max);
        }
        set
        {
            if(_channel == null)
                return;
            
            _channel.set3DMinMaxDistance(value.min, value.max);
        }
    }

    public float? MaxDistance;

    public bool IsPlaying
    {
        get
        {
            if (_channel == null)
                return false;

            _channel.isPlaying(out var isplaying);
            return isplaying;
        }
    }

    static SoundPlayer()
    {
        ClassInjector.RegisterTypeInIl2Cpp<SoundPlayer>();
    }

    private void Awake()
    {
        t = transform;
    }

    private void Update()
    {
        if (!IsPlaying || !t.hasChanged)
        {
            return;
        }

        var ch = _channel;
        SoundTools.SetPosition(ref ch, Audio3dAttributes.FromTransform(t));
    }

    public MODE ChannelMode = MODE._3D_WORLDRELATIVE;

    public Channel Play()
    {
        Stop();
        
        _channel = SoundTools.PlaySound(Sound, Audio3dAttributes.FromTransform(t), ChannelMode, MaxDistance);
        return _channel;
    }
    
    public void Stop()
    {
        if (!IsPlaying)
            return;

        _channel.stop();
        _channel = null;
    }
}