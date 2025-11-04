using GameFramework;
using GameFramework.DataTable;
using GameFramework.Sound;
using UnityGameFramework.Runtime;

namespace PuddingCat
{
    /// <summary>
    /// 声音相关的扩展方法。
    /// </summary>
    public static class SoundExtension
    {
        // 音乐淡入淡出的时长
        private const float FadeVolumeDuration = 1f;

        // 用于存储当前正在播放的音乐的序列号，确保全局只有一个BGM
        private static int? s_MusicSerialId = null;

        /// <summary>
        /// 扩展方法：播放背景音乐。
        /// </summary>
        /// <param name="soundComponent">SoundComponent 实例。</param>
        /// <param name="musicId">音乐的ID（在DRMusic数据表中定义）。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>返回音乐的序列号。</returns>
        public static int? PlayMusic(this SoundComponent soundComponent, int musicId, object userData = null)
        {
            // 播放新音乐前，先停止当前正在播放的音乐
            soundComponent.StopMusic();

            // 从数据表中查找音乐的配置信息
            IDataTable<DRMusic> dtMusic = GameEntry.DataTable.GetDataTable<DRMusic>();
            DRMusic drMusic = dtMusic.GetDataRow(musicId);
            if (drMusic == null)
            {
                Log.Warning("Can not load music '{0}' from data table.", musicId.ToString());
                return null;
            }

            // 创建播放参数
            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = 64; // 音乐的优先级
            playSoundParams.Loop = true; // 音乐通常是循环的
            playSoundParams.VolumeInSoundGroup = 1f; // 在音乐组内的音量
            playSoundParams.FadeInSeconds = FadeVolumeDuration; // 淡入时间
            playSoundParams.SpatialBlend = 0f; // 0f 代表是 2D 声音

            // 调用底层接口播放声音，并保存返回的序列号
            s_MusicSerialId = soundComponent.PlaySound(AssetUtility.GetMusicAsset(drMusic.AssetName), "Music",
                Constant.AssetPriority.MusicAsset, playSoundParams, null, userData);
            return s_MusicSerialId;
        }

        /// <summary>
        /// 扩展方法：停止背景音乐。
        /// </summary>
        public static void StopMusic(this SoundComponent soundComponent)
        {
            // 如果没有音乐正在播放，则直接返回
            if (!s_MusicSerialId.HasValue)
            {
                return;
            }

            // 调用底层接口停止声音，并附带淡出效果
            soundComponent.StopSound(s_MusicSerialId.Value, FadeVolumeDuration);
            // 清空序列号
            s_MusicSerialId = null;
        }

        /// <summary>
        /// 扩展方法：播放音效。
        /// </summary>
        /// <param name="soundId">音效的ID（在DRSound数据表中定义）。</param>
        /// <param name="bindingEntity">绑定的实体，如果非空，则音效为3D音效，会跟随实体移动。</param>
        /// <returns>返回音效的序列号。</returns>
        public static int? PlaySound(this SoundComponent soundComponent, int soundId, Entity bindingEntity = null,
            object userData = null)
        {
            IDataTable<DRSound> dtSound = GameEntry.DataTable.GetDataTable<DRSound>();
            DRSound drSound = dtSound.GetDataRow(soundId);
            if (drSound == null)
            {
                Log.Warning("Can not load sound '{0}' from data table.", soundId.ToString());
                return null;
            }

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            // 所有播放参数都从数据表中读取，实现完全的数据驱动
            playSoundParams.Priority = drSound.Priority;
            playSoundParams.Loop = drSound.Loop;
            playSoundParams.VolumeInSoundGroup = drSound.Volume;
            playSoundParams.SpatialBlend = drSound.SpatialBlend; // 空间混合度决定了声音是2D还是3D

            // 播放音效到 "Sound" 声音组
            return soundComponent.PlaySound(AssetUtility.GetSoundAsset(drSound.AssetName), "Sound",
                Constant.AssetPriority.SoundAsset, playSoundParams, bindingEntity != null ? bindingEntity.Entity : null,
                userData);
        }

        /// <summary>
        /// 扩展方法：播放UI音效。
        /// </summary>
        /// <param name="uiSoundId">UI音效的ID（在DRUISound数据表中定义）。</param>
        public static int? PlayUISound(this SoundComponent soundComponent, int uiSoundId, object userData = null)
        {
            IDataTable<DRUISound> dtUISound = GameEntry.DataTable.GetDataTable<DRUISound>();
            DRUISound drUISound = dtUISound.GetDataRow(uiSoundId);
            if (drUISound == null)
            {
                Log.Warning("Can not load UI sound '{0}' from data table.", uiSoundId.ToString());
                return null;
            }

            PlaySoundParams playSoundParams = PlaySoundParams.Create();
            playSoundParams.Priority = drUISound.Priority;
            playSoundParams.Loop = false; // UI 音效通常不循环
            playSoundParams.VolumeInSoundGroup = drUISound.Volume;
            playSoundParams.SpatialBlend = 0f; // UI 音效总是 2D 的

            // 播放音效到 "UISound" 声音组
            return soundComponent.PlaySound(AssetUtility.GetUISoundAsset(drUISound.AssetName), "UISound",
                Constant.AssetPriority.UISoundAsset, playSoundParams, userData);
        }

        /// <summary>
        /// 扩展方法：检查指定声音组是否静音。
        /// </summary>
        public static bool IsMuted(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return true;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return true;
            }

            return soundGroup.Mute;
        }

        /// <summary>
        /// 扩展方法：设置指定声音组的静音状态，并保存到本地设置。
        /// </summary>
        public static void Mute(this SoundComponent soundComponent, string soundGroupName, bool mute)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return;
            }

            // 1. 改变实时音效组的静音状态
            soundGroup.Mute = mute;

            // 2. 将这个设置持久化保存
            GameEntry.Setting.SetBool(Utility.Text.Format(Constant.Setting.SoundGroupMuted, soundGroupName), mute);
            GameEntry.Setting.Save();
        }

        /// <summary>
        /// 扩展方法：获取指定声音组的音量。
        /// </summary>
        public static float GetVolume(this SoundComponent soundComponent, string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return 0f;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.", soundGroupName);
                return 0f;
            }

            return soundGroup.Volume;
        }

        /// <summary>
        /// 扩展方法：设置指定声音组的音量，并保存到本地设置。
        /// </summary>
        public static void SetVolume(this SoundComponent soundComponent, string soundGroupName, float volume)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                Log.Warning("Sound group is invalid.");
                return;
            }

            ISoundGroup soundGroup = soundComponent.GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                Log.Warning("Sound group '{0}' is invalid.",
                    soundGroupName);
                return;
            }

            // 1. 改变实时音效组的音量
            soundGroup.Volume = volume;

            // 2. 将这个设置持久化保存
            GameEntry.Setting.SetFloat(Utility.Text.Format(Constant.Setting.SoundGroupVolume, soundGroupName), volume);
            GameEntry.Setting.Save();
        }
    }
}