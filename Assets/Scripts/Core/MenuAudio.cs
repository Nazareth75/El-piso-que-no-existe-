using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Música del menú y del recorrido, y los clics de los botones.
    /// </summary>
    public class MenuAudio : MonoBehaviour
    {
        public static MenuAudio Instance { get; private set; }

        const string MusicKey = "piso7.musica";
        const string SfxKey = "piso7.efectos";

        AudioSource _music;
        AudioSource _sfx;
        AudioClip _menu;
        AudioClip _campus;
        AudioClip _click;
        AudioClip _hover;
        bool _musicPaused;

        public static float MusicVolume { get; private set; } = 0.45f;
        public static float SfxVolume { get; private set; } = 0.7f;

        public static MenuAudio Ensure()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("SonidoMenu");
            return go.AddComponent<MenuAudio>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            MusicVolume = PlayerPrefs.GetFloat(MusicKey, 0.45f);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, 0.7f);

            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.spatialBlend = 0f;
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;

            _menu = Resources.Load<AudioClip>("Menu/musica");
            _campus = Resources.Load<AudioClip>("Menu/campus");
            _click = Resources.Load<AudioClip>("Menu/click");
            _hover = Resources.Load<AudioClip>("Menu/hover");
            ApplyVolumes();
        }

        public static void SetMusic(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            Instance?.ApplyVolumes();
        }

        public static void SetSfx(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            Instance?.ApplyVolumes();
        }

        public static void PlayMenu()
        {
            var audio = Ensure();
            audio._musicPaused = false;
            audio.PlayClip(audio._menu);
        }

        public static void PlayCampus()
        {
            var audio = Ensure();
            audio._musicPaused = false;
            audio.PlayClip(audio._campus != null ? audio._campus : audio._menu);
        }

        public static void PauseMusic()
        {
            var audio = Ensure();
            audio._musicPaused = true;
            audio._music.Pause();
        }

        public static void Click()
        {
            Ensure().PlayOne(Ensure()._click);
        }

        public static void Hover()
        {
            Ensure().PlayOne(Ensure()._hover);
        }

        void PlayClip(AudioClip clip)
        {
            if (clip == null)
                return;

            if (_music.clip == clip && _music.isPlaying)
            {
                ApplyVolumes();
                return;
            }

            _music.clip = clip;
            _music.volume = MusicVolume;
            _music.Play();
        }

        void PlayOne(AudioClip clip)
        {
            if (clip == null || SfxVolume <= 0.01f)
                return;
            _sfx.PlayOneShot(clip, SfxVolume);
        }

        void ApplyVolumes()
        {
            if (_music != null)
                _music.volume = MusicVolume;
        }
    }
}
