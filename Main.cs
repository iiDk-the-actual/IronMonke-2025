using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace IronMonke
{
    public class ModInfo
    {
        public const string _id = "buzzbb.ironmonke";
        public const string _name = "Iron Monke";
    }

    [BepInPlugin(ModInfo._id, ModInfo._name, "1.0.6")]
    public class Main : BaseUnityPlugin
    {
        GameObject gL;
        AudioSource aL;
        ParticleSystem psL;
        GameObject gR;
        AudioSource aR;
        ParticleSystem psR;

        void Start()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            GorillaTagger.OnPlayerSpawned(delegate { OnGameInitialized(); });
        }

        void OnGameInitialized()
        {
            gL = LoadAsset("gloveL");
            aL = gL.GetComponent<AudioSource>();
            gL.transform.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body_pivot/hand.L"), false);
            psL = gL.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
            gL?.SetActive(true);

            gR = LoadAsset("gloveR");
            aR = gR.GetComponent<AudioSource>();
            gR.transform.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body_pivot/hand.R"), false);
            psR = gR.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
            gR.SetActive(true);
        }

        private AssetBundle assetBundle;
        private void LoadAssetBundle()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("IronMonke.Resources.gloven");
            if (stream != null)
                assetBundle = AssetBundle.LoadFromStream(stream);
            else
                Debug.LogError("Failed to load assetbundle");
        }

        public GameObject LoadAsset(string assetName)
        {
            GameObject gameObject = null;

            if (assetBundle == null)
                LoadAssetBundle();

            gameObject = UnityEngine.Object.Instantiate(assetBundle.LoadAsset<GameObject>(assetName));

            return gameObject;
        }

        void FixedUpdate()
        {
            try
            {
                if (ControllerInputPoller.instance.leftControllerSecondaryButton)
                {
                    GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(10 * gL.transform.parent.right, ForceMode.Acceleration);
                    if (!psL.isPlaying) psL.Play();
                    if (!aL.isPlaying) aL.Play();
                    GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 50f * GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    aL.volume = 0.03f * GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity.magnitude;
                }
                else
                {
                    psL.Stop();
                    aL.Stop();
                }

                if (ControllerInputPoller.instance.rightControllerSecondaryButton)
                {
                    GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(10 * -gR.transform.parent.right, ForceMode.Acceleration);
                    if (!psR.isPlaying) psR.Play();
                    if (!aR.isPlaying) aR.Play();
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 50f * GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity.magnitude, GorillaTagger.Instance.tapHapticDuration);
                    aR.volume = 0.03f * GorillaLocomotion.GTPlayer.Instance.bodyCollider.attachedRigidbody.linearVelocity.magnitude;
                }
                else
                {
                    psR.Stop();
                    aR.Stop();
                }
            }
            catch { }
        }
    }

    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                if (instance == null)
                {
                    instance = new Harmony(ModInfo._id);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}