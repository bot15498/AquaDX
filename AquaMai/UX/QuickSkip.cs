﻿using System.Collections.Generic;
using HarmonyLib;
using Mai2.Mai2Cue;
using MAI2.Util;
using Main;
using Manager;
using MelonLoader;
using Process;
using Process.Information;
using UnityEngine;

namespace AquaMai.UX
{
    public class QuickSkip
    {
        private static ProcessDataContainer _container;
        private static int _keyPressFrames;
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcessDataContainer), MethodType.Constructor)]
        public static void OnCreateProcessDataContainer(ProcessDataContainer __instance)
        {
            _container = __instance;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameMainObject), "Update")]
        public static void OnGameMainObjectUpdate()
        {
            // The button between [1p] and [2p] button on ADX
            if (Input.GetKey(KeyCode.Alpha7)) _keyPressFrames++;
            
            if (Input.GetKeyUp(KeyCode.Alpha7))
            {
                _keyPressFrames = 0;
                MelonLogger.Msg(_container.processManager.Dump());
                return;
            }

            if (_keyPressFrames != 60) return;
            
            var traverse = Traverse.Create(_container.processManager);
            var processList = traverse.Field("_processList").GetValue<LinkedList<ProcessManager.ProcessControle>>();
            
            var flagGoToMusicSelect = false;
            
            foreach (ProcessManager.ProcessControle process in processList)
            {
                switch (process.Process.ToString())
                {
                    // After login
                    case "Process.ModeSelect.ModeSelectProcess":
                    case "Process.RegionalSelectProcess":
                    case "Process.CharacterSelectProcess":
                    case "Process.TicketSelect.TicketSelectProcess":
                    // After playing a song
                    case "Process.ResultProcess":
                    case "Process.MapResultProcess":
                        _container.processManager.ReleaseProcess(process.Process);
                        flagGoToMusicSelect = true;
                        break;
                    
                    case "Process.MusicSelectProcess":
                        // Skip to save
                        SoundManager.PreviewEnd();
                        SoundManager.PlayBGM(Cue.BGM_COLLECTION, 2);
                        _container.processManager.ReleaseProcess(process.Process);
                        _container.processManager.AddProcess(new UnlockMusicProcess(_container));
                        break;
                    
                    case "Process.GameProcess":
                        // This is original typo in Assembly-CSharp
                        Singleton<GamePlayManager>.Instance.SetQuickRetryFrag(flag: true);
                        break;
                }
            }

            if (flagGoToMusicSelect)
            {
                GameManager.SetMaxTrack();
                _container.processManager.AddProcess(new MusicSelectProcess(_container));
            }
        }

    }
}
