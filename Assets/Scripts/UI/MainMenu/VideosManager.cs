using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace Sistemata.UI.MainMenu
{
    public class VideosManager : MonoBehaviour
    {
        [Header("Media")] [SerializeField] private VideoClip[] videos = { };
        [SerializeField] private float transitionTime = 1.5f;

        [Header("References")] [SerializeField]
        private VideoPlayer playerA;

        [SerializeField] private VideoPlayer playerB;
        [SerializeField] private CanvasGroup groupA;
        [SerializeField] private CanvasGroup groupB;

        private bool _playerAInUse = true;
        private int _videoIdx = -1;

        private void Start()
        {
            if (videos.Length > 0)
            {
                PlayNextVideo();
            }
        }

        private void Update()
        {
            var activatePlayer = _playerAInUse ? playerA : playerB;

            if (activatePlayer.isPlaying && activatePlayer.time >= activatePlayer.length - transitionTime)
            {
                PlayNextVideo();
            }
        }

        private void PlayNextVideo()
        {
            int newIdx;
            do
            {
                newIdx = Random.Range(0, videos.Length);
            } while (newIdx == _videoIdx && videos.Length > 1);

            _videoIdx = newIdx;
            var proximoClip = videos[_videoIdx];

            if (_playerAInUse)
            {
                playerB.clip = proximoClip;
                playerB.Play();
                StartCoroutine(Crossfade(groupA, groupB));
            }
            else
            {
                playerA.clip = proximoClip;
                playerA.Play();
                StartCoroutine(Crossfade(groupB, groupA));
            }

            _playerAInUse = !_playerAInUse;
        }

        private IEnumerator Crossfade(CanvasGroup current, CanvasGroup next)
        {
            var elapsedTime = 0f;

            while (elapsedTime < transitionTime)
            {
                elapsedTime += Time.deltaTime;
                var progress = elapsedTime / transitionTime;

                current.alpha = Mathf.Lerp(1f, 0f, progress);
                next.alpha = Mathf.Lerp(0f, 1f, progress);

                yield return null;
            }

            current.alpha = 0f;
            next.alpha = 1f;

            if (current == groupA) playerA.Stop();
            else playerB.Stop();
        }
    }
}
