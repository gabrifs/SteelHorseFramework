using UnityEngine;

namespace SteelHorse.Framework.Services.Audio
{
    public class PlaylistAutoStarter : MonoBehaviour
    {
        [SerializeField] private MusicPlaylist _playlist;

        private void Start()
        {
            GameManagers.Instance.Services.MusicPlayerService.Play(_playlist);
        }
    }
}
