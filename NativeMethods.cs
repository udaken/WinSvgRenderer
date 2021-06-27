﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WinSvgRenderer
{
    // https://docs.microsoft.com/ja-jp/previous-versions/windows/internet-explorer/ie-developer/platform-apis/ms537184(v=vs.85)
    internal static class NativeMethods
    {
        public enum INTERNETFEATURELIST
        {
            FEATURE_OBJECT_CACHING = 0,
            FEATURE_ZONE_ELEVATION = 1,
            FEATURE_MIME_HANDLING = 2,
            FEATURE_MIME_SNIFFING = 3,
            FEATURE_WINDOW_RESTRICTIONS = 4,
            FEATURE_WEBOC_POPUPMANAGEMENT = 5,
            FEATURE_BEHAVIORS = 6,
            FEATURE_DISABLE_MK_PROTOCOL = 7,
            FEATURE_LOCALMACHINE_LOCKDOWN = 8,
            FEATURE_SECURITYBAND = 9,
            FEATURE_RESTRICT_ACTIVEXINSTALL = 10,
            FEATURE_VALIDATE_NAVIGATE_URL = 11,
            FEATURE_RESTRICT_FILEDOWNLOAD = 12,
            FEATURE_ADDON_MANAGEMENT = 13,
            FEATURE_PROTOCOL_LOCKDOWN = 14,
            FEATURE_HTTP_USERNAME_PASSWORD_DISABLE = 15,
            FEATURE_SAFE_BINDTOOBJECT = 16,
            FEATURE_UNC_SAVEDFILECHECK = 17,
            FEATURE_GET_URL_DOM_FILEPATH_UNENCODED = 18,
            FEATURE_TABBED_BROWSING = 19,
            FEATURE_SSLUX = 20,
            FEATURE_DISABLE_NAVIGATION_SOUNDS = 21,
            FEATURE_DISABLE_LEGACY_COMPRESSION = 22,
            FEATURE_FORCE_ADDR_AND_STATUS = 23,
            FEATURE_XMLHTTP = 24,
            FEATURE_DISABLE_TELNET_PROTOCOL = 25,
            FEATURE_FEEDS = 26,
            FEATURE_BLOCK_INPUT_PROMPTS = 27,
            FEATURE_ENTRY_COUNT = 28
        }

        public const int SET_FEATURE_ON_THREAD = 0x00000001;
        public const int SET_FEATURE_ON_PROCESS = 0x00000002;
        public const int SET_FEATURE_IN_REGISTRY = 0x00000004;
        public const int SET_FEATURE_ON_THREAD_LOCALMACHINE = 0x00000008;
        public const int SET_FEATURE_ON_THREAD_INTRANET = 0x00000010;
        public const int SET_FEATURE_ON_THREAD_TRUSTED = 0x00000020;
        public const int SET_FEATURE_ON_THREAD_INTERNET = 0x00000040;
        public const int SET_FEATURE_ON_THREAD_RESTRICTED = 0x00000080;

        public const int GET_FEATURE_FROM_THREAD = 0x00000001;
        public const int GET_FEATURE_FROM_PROCESS = 0x00000002;
        public const int GET_FEATURE_FROM_REGISTRY = 0x00000004;
        public const int GET_FEATURE_FROM_THREAD_LOCALMACHINE = 0x00000008;
        public const int GET_FEATURE_FROM_THREAD_INTRANET = 0x00000010;
        public const int GET_FEATURE_FROM_THREAD_TRUSTED = 0x00000020;
        public const int GET_FEATURE_FROM_THREAD_INTERNET = 0x00000040;
        public const int GET_FEATURE_FROM_THREAD_RESTRICTED = 0x00000080;

        [Flags]
        public enum GetFeatureFlag : uint
        {
            None = 0,
            FromThread = GET_FEATURE_FROM_THREAD,
            FromProcess = GET_FEATURE_FROM_PROCESS,
            FromRegistry = GET_FEATURE_FROM_REGISTRY,
            FromThreadLocalMachine = GET_FEATURE_FROM_THREAD_LOCALMACHINE,
            FromThreadIntranet = GET_FEATURE_FROM_THREAD_INTRANET,
            FromThreadTrusted = GET_FEATURE_FROM_THREAD_TRUSTED,
            FromThreadInternet = GET_FEATURE_FROM_THREAD_INTERNET,
            FromThreadRestricted = GET_FEATURE_FROM_THREAD_RESTRICTED,
        }

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        public static extern int CoInternetIsFeatureEnabled(
            INTERNETFEATURELIST FeatureEntry,
              GetFeatureFlag dwFlags);

        [Flags]
        public enum SetFeatureFlag : uint
        {
            None = 0,
            OnThread = SET_FEATURE_ON_THREAD,
            OnProcess = SET_FEATURE_ON_PROCESS,
            InRegistry = SET_FEATURE_IN_REGISTRY,
            OnThreadLocalMachine = SET_FEATURE_ON_THREAD_LOCALMACHINE,
            OnThreadIntranet = SET_FEATURE_ON_THREAD_INTRANET,
            OnThreadTrusted = SET_FEATURE_ON_THREAD_TRUSTED,
            OnThreadInternet = SET_FEATURE_ON_THREAD_INTERNET,
            OnThreadRestricted = SET_FEATURE_ON_THREAD_RESTRICTED,
        }

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        public static extern int CoInternetSetFeatureEnabled(
             INTERNETFEATURELIST FeatureEntry,
             SetFeatureFlag dwFlags,
             [MarshalAs(UnmanagedType.Bool)] bool fEnable);
    }
}
