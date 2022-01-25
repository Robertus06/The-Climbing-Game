using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    // if _Amplitude value is more than 0.5f microphone is recording something
    
    [SerializeField] private AudioSource _audioSource;

    private float[] _samplesLeft = new float[512];
    private float[] _samplesRight = new float[512];
    
    private float[] _freqBand = new float[8];
    private float[] _bandBuffer = new float [8];
    private float[] _bufferDecrease = new float[8];
    private float[] _freqBandHighest = new float[8];
    
    private float[] _freqBand64 = new float[64];
    private float[] _bandBuffer64 = new float [64];
    private float[] _bufferDecrease64 = new float[64];
    private float[] _freqBandHighest64 = new float[64];
    
    [HideInInspector]
    public float[] _audioBand, _audioBandBuffer;

    [HideInInspector]
    public float[] _audioBand64, _audioBandBuffer64;
    
    [HideInInspector]
    public float _Amplitude, _AmplitudeBuffer;
    private float _AmplitudeHighest;
    public float _audioProfile;

    public enum _channel
    {
        Stereo,
        Left,
        Right
    };
    public _channel channel = new _channel();

    private AudioClip _AudioClip;
    [SerializeField] private string _selectedDevide;
    public AudioMixerGroup _mixerGroupMicrophone;

    // Start is called before the first frame update
    void Start()
    {
        _audioBand = new float[8];
        _audioBandBuffer = new float[8];
        _audioBand64 = new float[64];
        _audioBandBuffer64 = new float[64];
        
        _audioSource = GetComponent<AudioSource>();
        AudioProfile(_audioProfile);

        if (Microphone.devices.Length > 0)
        {
            _selectedDevide = Microphone.devices[0].ToString();
            _audioSource.outputAudioMixerGroup = _mixerGroupMicrophone;
            _audioSource.clip = Microphone.Start(_selectedDevide, true, 10, AudioSettings.outputSampleRate);
        }
        
        _audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeFrequencyBands64();
        BandBuffer();
        BandBuffer64();
        CreateAudioBands();
        CreateAudioBands64();
        GetAmplitude();
    }

    void AudioProfile(float audioProfile)
    {
        for (int i = 0; i < 8; i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }

    void GetAmplitude()
    {
        float _CurrentAmplitude = 0;
        float _CurrentAmplitudeBuffer = 0;
        for (int i = 0; i < 8; i++)
        {
            _CurrentAmplitude += _audioBand[i];
            _CurrentAmplitudeBuffer += _audioBandBuffer[i];
        }
        if (_CurrentAmplitude > _AmplitudeHighest)
        {
            _AmplitudeHighest = _CurrentAmplitude;
        }
        _Amplitude = _CurrentAmplitude / _AmplitudeHighest;
        _AmplitudeBuffer = _CurrentAmplitudeBuffer / _AmplitudeHighest;
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_freqBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _freqBand[i];
            }

            _audioBand[i] = (_freqBand[i] / _freqBandHighest[i]);
            _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }
    
    void CreateAudioBands64()
    {
        for (int i = 0; i < 64; i++)
        {
            if (_freqBand64[i] > _freqBandHighest64[i])
            {
                _freqBandHighest64[i] = _freqBand64[i];
            }

            _audioBand64[i] = (_freqBand64[i] / _freqBandHighest64[i]);
            _audioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
        }
    }

    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int) Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if (channel == _channel.Stereo)
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                if (channel == _channel.Left)
                    average += _samplesLeft[count] * (count + 1);
                if (channel == _channel.Right)
                    average += _samplesRight[count] * (count + 1);

                count++;
            }

            average /= count;

            _freqBand[i] = average * 10;
        }
    }
    
    void MakeFrequencyBands64()
    {
        int count = 0;
        int sampleCount = 1;
        int power = 0;
        
        for (int i = 0; i < 64; i++)
        {
            float average = 0;
            //int sampleCount = (int) Mathf.Pow(2, i) * 2;

            if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
            {
                power++;
                sampleCount = (int) Mathf.Pow(2, power);
                if (power == 3)
                {
                    sampleCount -= 2;
                }
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if (channel == _channel.Stereo)
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                if (channel == _channel.Left)
                    average += _samplesLeft[count] * (count + 1);
                if (channel == _channel.Right)
                    average += _samplesRight[count] * (count + 1);

                count++;
            }

            average /= count;

            _freqBand64[i] = average * 80;
        }
    }

    void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_freqBand[i] > _bandBuffer[i])
            {
                _bandBuffer[i] = _freqBand[i];
                _bufferDecrease[i] = 0.005f;
            }

            if (_freqBand[i] < _bandBuffer[i])
            {
                _bandBuffer[i] -= _bufferDecrease[i];
                _bufferDecrease[i] *= 1.2f;
            }
        }
    }
    
    void BandBuffer64()
    {
        for (int i = 0; i < 64; i++)
        {
            if (_freqBand64[i] > _bandBuffer64[i])
            {
                _bandBuffer64[i] = _freqBand64[i];
                _bufferDecrease64[i] = 0.005f;
            }

            if (_freqBand64[i] < _bandBuffer64[i])
            {
                _bandBuffer64[i] -= _bufferDecrease64[i];
                _bufferDecrease64[i] *= 1.2f;
            }
        }
    }
}