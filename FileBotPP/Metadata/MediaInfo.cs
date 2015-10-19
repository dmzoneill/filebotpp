using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using FileBotPP.Interfaces;
using FileBotPP.Tree;

namespace FileBotPP.Metadata
{
    public class MediaInfo : INotifyPropertyChanged, IMediaInfo
    {
        private static readonly string[] IgnoredTags =
        {
            "DURATION", "NUMBER_OF_FRAMES", "NUMBER_OF_BYTES", "STATISTICS_WRITING_APP", "STATISTICS_WRITING_DATE_UTC", "STATISTICS_TAGS", "_STATISTICS_TAGS", "_STATISTICS_WRITING_APP", "_STATISTICS_WRITING_DATE_UTC"
        };

        private static readonly Dictionary< string, string > Audio;
        private static readonly Dictionary< string, string > General;
        private static readonly Dictionary< string, string > Video;
        private static readonly Dictionary< string, string > Text;
        private string _audioAlignment;
        private string _audioBitrate;
        private string _audioBitratemode;
        private string _audioChannel;
        private string _audioChannelPositions;
        private string _audioCodecId;
        private string _audioCodecIdHint;
        private string _audioCompressionmode;
        private string _audioDefault;
        private string _audioDelayRelativeToVideo;
        private string _audioDuration;
        private string _audioEncodedDate;
        private string _audioEncodingsettings;
        private string _audioForced;
        private string _audioFormat;
        private string _audioFormatInfo;
        private string _audioFormatprofile;
        private string _audioFormatsettingEndianness;
        private string _audioFormatversion;
        private string _audioId;
        private string _audioInterleaveduration;
        private string _audioInterleavepreloadduration;
        private string _audioLanguage;
        private string _audioMaximumBitRate;
        private string _audioMinimumBitRate;
        private string _audioMode;
        private string _audioModeextension;
        private string _audioNominalbitrate;
        private string _audioSamplingRate;
        private string _audioSourceDuration;
        private string _audioStreamSize;
        private string _audioTaggedDate;
        private string _audioWritinglibrary;
        private string _generalCodecId;
        private string _generalComment;
        private string _generalCompletename;
        private string _generalDuration;
        private string _generalEncodeddate;
        private string _generalFilesize;
        private string _generalFormat;
        private string _generalFormatInfo;
        private string _generalFormatProfile;
        private string _generalFormatversion;
        private string _generalOverallbitrate;
        private string _generalOverallbitratemode;
        private string _generalTaggedDate;
        private string _generalUniqueId;
        private string _generalWritingapplication;
        private string _generalWritinglibrary;
        private string _textCodecId;
        private string _textCodecIdInfo;
        private string _textDefault;
        private string _textForced;
        private string _textFormat;
        private string _textId;
        private string _textLanguage;
        private string _textTitle;
        private string _videoBitdepth;
        private string _videoBitrate;
        private string _videoBitRateMode;
        private string _videoBitsPixelFrame;
        private string _videoChromasubsampling;
        private string _videoCodecId;
        private string _videoCodecIdHint;
        private string _videoCodecIdInfo;
        private string _videoCodecSource;
        private string _videoColorrange;
        private string _videoColorspace;
        private string _videoCompressionMode;
        private string _videoDefault;
        private string _videoDisplayaspectratio;
        private string _videoDuration;
        private string _videoEncodedDate;
        private string _videoEncodingsettings;
        private string _videoForced;
        private string _videoFormat;
        private string _videoFormatInfo;
        private string _videoFormatprofile;
        private string _videoFormatsettingsBvop;
        private string _videoFormatsettingsCabac;
        private string _videoFormatsettingsGmc;
        private string _videoFormatsettingsGop;
        private string _videoFormatsettingsMatrix;
        private string _videoFormatSettingsPictureStructure;
        private string _videoFormatsettingsQpel;
        private string _videoFormatsettingsReFrames;
        private string _videoFormatVersion;
        private string _videoFramerate;
        private string _videoFrameratemode;
        private string _videoGopOpenClosed;
        private string _videoGopOpenClosedOfFirstFrame;
        private string _videoHeight;
        private string _videoId;
        private string _videoLanguage;
        private string _videoMatrixcoefficients;
        private string _videoMaximumBitRate;
        private string _videoMuxingmode;
        private string _videoNominalBitRate;
        private string _videoScanOrder;
        private string _videoScantype;
        private string _videoSourceDuration;
        private string _videoStandard;
        private string _videoStreamsize;
        private string _videoTaggedDate;
        private string _videoTimeCodeOfFirstFrame;
        private string _videoWidth;
        private string _videoWritinglibrary;

        static MediaInfo()
        {
            General = new Dictionary< string, string >
            {
                {"Unique ID", "GeneralUniqueId"},
                {"Complete name", "GeneralCompleteName"},
                {"Comment", "GeneralComment"},
                {"Format", "GeneralFormat"},
                {"Format version", "GeneralFormatVersion"},
                {"File size", "GeneralFileSize"},
                {"Duration", "GeneralDuration"},
                {"Overall bit rate", "GeneralOverallBitRate"},
                {"Overall bit rate mode", "GeneralOverallBitRateMode"},
                {"Encoded date", "GeneralEncodedDate"},
                {"Writing application", "GeneralWritingApplication"},
                {"Writing library", "GeneralWritingLibrary"},
                {"Format/Info", "GeneralFormatInfo"},
                {"Format profile", "GeneralFormatProfile"},
                {"Codec ID", "GeneralCodecId"},
                {"Tagged date", "GeneralTaggedDate"}
            };

            Video = new Dictionary< string, string >
            {
                {"ID", "VideoId"},
                {"Format", "VideoFormat"},
                {"Format/Info", "VideoFormatInfo"},
                {"Format profile", "VideoFormatProfile"},
                {"Format version", "VideoFormatversion"},
                {"Format settings, BVOP", "VideoFormatSettingsBvop"},
                {"Format settings, QPel", "VideoFormatSettingsQpel"},
                {"Format settings, GMC", "VideoFormatSettingsGmc"},
                {"Format settings, Matrix", "VideoFormatSettingsMatrix"},
                {"Format settings, CABAC", "VideoFormatSettingsCabac"},
                {"Format settings, ReFrames", "VideoFormatSettingsReFrames"},
                {"Format settings, GOP", "VideoFormatSettingsGop"},
                {"Muxing mode", "VideoMuxingMode"},
                {"Codec ID", "VideoCodecId"},
                {"Codec ID/Hint", "VideoCodecIdHint"},
                {"Codec ID/Info", "VideoCodecIdInfo"},
                {"Duration", "VideoDuration"},
                {"Source duration", "VideoSourceDuration"},
                {"Bit rate", "VideoBitRate"},
                {"Width", "VideoWidth"},
                {"Height", "VideoHeight"},
                {"Display aspect ratio", "VideoDisplayAspectRatio"},
                {"Frame rate mode", "VideoFrameRateMode"},
                {"Frame rate", "VideoFrameRate"},
                {"Color space", "VideoColorSpace"},
                {"Chroma subsampling", "VideoChromaSubSampling"},
                {"Bit depth", "VideoBitDepth"},
                {"Compression mode", "VideoCompressionMode"},
                {"Scan type", "VideoScanType"},
                {"Bits/(Pixel*Frame)", "VideoBitsPixelFrame"},
                {"Stream size", "VideoStreamSize"},
                {"Writing library", "VideoWritingLibrary"},
                {"Encoding settings", "VideoEncodingSettings"},
                {"Language", "VideoLanguage"},
                {"Default", "VideoDefault"},
                {"Forced", "VideoForced"},
                {"Color range", "VideoColorRange"},
                {"Standard", "VideoStandard"},
                {"Matrix coefficients", "VideoMatrixCoefficients"},
                {"Time code source", "VideoTimeCodeSource"},
                {"Time code of first frame", "VideoTimeCodeOfFirstFrame"},
                {"GOP, Open/Closed", "VideoGopOpenClosed"},
                {"GOP, Open/Closed of first frame", "VideoGopOpenClosedOfFirstFrame"},
                {"Maximum bit rate", "VideoMaximumBitRate"},
                {"Encoded date", "VideoEncodedDate"},
                {"Tagged date", "VideoTaggedDate"},
                {"Nominal bit rate", "VideoNominalBitRate"},
                {"Format settings, picture structure", "VideoFormatSettingsPictureStructure"},
                {"Bit rate mode", "VideoBitRateMode"},
                {"Scan order", "VideoScanOrder"}
            };

            Audio = new Dictionary< string, string >
            {
                {"ID", "AudioId"},
                {"Format", "AudioFormat"},
                {"Format version", "AudioFormatVersion"},
                {"Format/Info", "AudioFormatInfo"},
                {"Format profile", "AudioFormatProfile"},
                {"Mode", "AudioMode"},
                {"Mode extension", "AudioModeExtension"},
                {"Format settings, Endianness", "AudioFormatSettingEndianness"},
                {"Nominal bit rate", "AudioNominalBitRate"},
                {"Codec ID", "AudioCodecId"},
                {"Codec ID/Hint", "AudioCodecIdHint"},
                {"Duration", "AudioDuration"},
                {"Source duration", "AudioSourceDuration"},
                {"Bit rate mode", "AudioBitRateMode"},
                {"Bit rate", "AudioBitRate"},
                {"Channel(s)", "AudioChannel"},
                {"Channel positions", "AudioChannelPositions"},
                {"Sampling rate", "AudioSamplingRate"},
                {"Compression mode", "AudioCompressionMode"},
                {"Stream size", "AudioStreamSize"},
                {"Default", "AudioDefault"},
                {"Forced", "AudioForced"},
                {"Language", "AudioLanguage"},
                {"Alignment", "AudioAlignment"},
                {"Interleave, duration", "AudioInterLeaveDuration"},
                {"Interleave, preload duration", "AudioInterleavePreloadDuration"},
                {"Writing library", "AudioWritingLibrary"},
                {"Encoding settings", "AudioEncodingSettings"},
                {"Delay relative to video", "AudioDelayRelativeToVideo"},
                {"Maximum bit rate", "AudioMaximumBitRate"},
                {"Encoded date", "AudioEncodedDate"},
                {"Minimum bit rate", "AudioMinimumBitRate"},
                {"Tagged date", "AudioTaggedDate"}
            };


            Text = new Dictionary< string, string >
            {
                {"ID", "TextId"},
                {"Format", "TextFormat"},
                {"Codec ID", "TextCodecId"},
                {"Codec ID/Info", "TextCodecIdInfo"},
                {"Title", "TextTitle"},
                {"Language", "TextLanguage"},
                {"Default", "TextDefault"},
                {"Forced", "TextForced"}
            };
        }

        public MediaInfo( IFileItem file, string mediainfo )
        {
            this.parse_media_info( mediainfo );
            file.Mediainfo = this;
            file.Update();
        }

        public string GeneralUniqueId
        {
            get { return this._generalUniqueId; }
            set
            {
                this._generalUniqueId = value;
                this.OnPropertyChanged( "GeneralUniqueId" );
            }
        }

        public string GeneralCompleteName
        {
            get { return this._generalCompletename; }
            set
            {
                this._generalCompletename = value;
                this.OnPropertyChanged( "GeneralCompleteName" );
            }
        }

        public string GeneralComment
        {
            get { return this._generalComment; }
            set
            {
                this._generalComment = value;
                this.OnPropertyChanged( "GeneralComment" );
            }
        }

        public string GeneralFormat
        {
            get { return this._generalFormat; }
            set
            {
                this._generalFormat = value;
                this.OnPropertyChanged( "GeneralFormat" );
            }
        }

        public string GeneralFormatVersion
        {
            get { return this._generalFormatversion; }
            set
            {
                this._generalFormatversion = value;
                this.OnPropertyChanged( "GeneralFormatVersion" );
            }
        }

        public string GeneralFileSize
        {
            get { return this._generalFilesize; }
            set
            {
                this._generalFilesize = value;
                this.OnPropertyChanged( "GeneralFileSize" );
            }
        }

        public string GeneralDuration
        {
            get { return this._generalDuration; }
            set
            {
                this._generalDuration = value;
                this.OnPropertyChanged( "GeneralDuration" );
            }
        }

        public string GeneralOverallBitRate
        {
            get { return this._generalOverallbitrate; }
            set
            {
                this._generalOverallbitrate = value;
                this.OnPropertyChanged( "GeneralOverallBitRate" );
            }
        }

        public string GeneralOverallBitRateMode
        {
            get { return this._generalOverallbitratemode; }
            set
            {
                this._generalOverallbitratemode = value;
                this.OnPropertyChanged( "GeneralOverallBitRateMode" );
            }
        }

        public string GeneralEncodedDate
        {
            get { return this._generalEncodeddate; }
            set
            {
                this._generalEncodeddate = value;
                this.OnPropertyChanged( "GeneralEncodedDate" );
            }
        }

        public string GeneralWritingApplication
        {
            get { return this._generalWritingapplication; }
            set
            {
                this._generalWritingapplication = value;
                this.OnPropertyChanged( "GeneralWritingApplication" );
            }
        }

        public string GeneralWritingLibrary
        {
            get { return this._generalWritinglibrary; }
            set
            {
                this._generalWritinglibrary = value;
                this.OnPropertyChanged( "GeneralWritingLibrary" );
            }
        }

        public string GeneralFormatInfo
        {
            get { return this._generalFormatInfo; }
            set
            {
                this._generalFormatInfo = value;
                this.OnPropertyChanged( "GeneralFormatInfo" );
            }
        }

        public string GeneralFormatProfile
        {
            get { return this._generalFormatProfile; }
            set
            {
                this._generalFormatProfile = value;
                this.OnPropertyChanged( "GeneralFormatProfile" );
            }
        }

        public string GeneralCodecId
        {
            get { return this._generalCodecId; }
            set
            {
                this._generalCodecId = value;
                this.OnPropertyChanged( "GeneralCodecId" );
            }
        }

        public string GeneralTaggedDate
        {
            get { return this._generalTaggedDate; }
            set
            {
                this._generalTaggedDate = value;
                this.OnPropertyChanged( "GeneralTaggedDate" );
            }
        }

        public string VideoId
        {
            get { return this._videoId; }
            set
            {
                this._videoId = value;
                this.OnPropertyChanged( "VideoId" );
            }
        }

        public string VideoFormat
        {
            get { return this._videoFormat; }
            set
            {
                this._videoFormat = value;
                this.OnPropertyChanged( "VideoFormat" );
            }
        }

        public string VideoStandard
        {
            get { return this._videoStandard; }
            set
            {
                this._videoStandard = value;
                this.OnPropertyChanged( "VideoStandard" );
            }
        }

        public string VideoFormatInfo
        {
            get { return this._videoFormatInfo; }
            set
            {
                this._videoFormatInfo = value;
                this.OnPropertyChanged( "VideoFormatInfo" );
            }
        }

        public string VideoFormatProfile
        {
            get { return this._videoFormatprofile; }
            set
            {
                this._videoFormatprofile = value;
                this.OnPropertyChanged( "VideoFormatProfile" );
            }
        }

        public string VideoFormatVersion
        {
            get { return this._videoFormatVersion; }
            set
            {
                this._videoFormatVersion = value;
                this.OnPropertyChanged( "VideoFormatVersion" );
            }
        }

        public string VideoFormatSettingsBvop
        {
            get { return this._videoFormatsettingsBvop; }
            set
            {
                this._videoFormatsettingsBvop = value;
                this.OnPropertyChanged( "VideoFormatSettingsBvop" );
            }
        }

        public string VideoFormatSettingsQpel
        {
            get { return this._videoFormatsettingsQpel; }
            set
            {
                this._videoFormatsettingsQpel = value;
                this.OnPropertyChanged( "VideoFormatSettingsQpel" );
            }
        }

        public string VideoFormatSettingsGmc
        {
            get { return this._videoFormatsettingsGmc; }
            set
            {
                this._videoFormatsettingsGmc = value;
                this.OnPropertyChanged( "VideoFormatSettingsGmc" );
            }
        }

        public string VideoFormatSettingsMatrix
        {
            get { return this._videoFormatsettingsMatrix; }
            set
            {
                this._videoFormatsettingsMatrix = value;
                this.OnPropertyChanged( "VideoFormatSettingsMatrix" );
            }
        }

        public string VideoFormatSettingsCabac
        {
            get { return this._videoFormatsettingsCabac; }
            set
            {
                this._videoFormatsettingsCabac = value;
                this.OnPropertyChanged( "VideoFormatSettingsCabac" );
            }
        }

        public string VideoFormatSettingsReFrames
        {
            get { return this._videoFormatsettingsReFrames; }
            set
            {
                this._videoFormatsettingsReFrames = value;
                this.OnPropertyChanged( "VideoFormatSettingsReFrames" );
            }
        }

        public string VideoFormatSettingsGop
        {
            get { return this._videoFormatsettingsGop; }
            set
            {
                this._videoFormatsettingsGop = value;
                this.OnPropertyChanged( "VideoFormatSettingsGop" );
            }
        }

        public string VideoMuxingMode
        {
            get { return this._videoMuxingmode; }
            set
            {
                this._videoMuxingmode = value;
                this.OnPropertyChanged( "VideoMuxingMode" );
            }
        }

        public string VideoCodecId
        {
            get { return this._videoCodecId; }
            set
            {
                this._videoCodecId = value;
                this.OnPropertyChanged( "VideoCodecId" );
            }
        }

        public string VideoCodecIdHint
        {
            get { return this._videoCodecIdHint; }
            set
            {
                this._videoCodecIdHint = value;
                this.OnPropertyChanged( "VideoCodecIdHint" );
            }
        }

        public string VideoCodecIdInfo
        {
            get { return this._videoCodecIdInfo; }
            set
            {
                this._videoCodecIdInfo = value;
                this.OnPropertyChanged( "VideoCodecIdInfo" );
            }
        }

        public string VideoDuration
        {
            get { return this._videoDuration; }
            set
            {
                this._videoDuration = value;
                this.OnPropertyChanged( "VideoDuration" );
            }
        }

        public string VideoSourceDuration
        {
            get { return this._videoSourceDuration; }
            set
            {
                this._videoSourceDuration = value;
                this.OnPropertyChanged( "VideoSourceDuration" );
            }
        }

        public string VideoBitRate
        {
            get { return this._videoBitrate; }
            set
            {
                this._videoBitrate = value;
                this.OnPropertyChanged( "VideoBitRate" );
            }
        }

        public string VideoWidth
        {
            get
            {
                var temp = this._videoWidth.Replace( " ", "" );
                var match = Regex.Match( temp, @"\d+" );
                return match.Success ? match.Value : this._videoWidth;
            }
            set
            {
                this._videoWidth = value;
                this.OnPropertyChanged( "VideoWidth" );
            }
        }

        public string VideoHeight
        {
            get
            {
                var temp = this._videoHeight.Replace( " ", "" );
                var match = Regex.Match( temp, @"\d+" );
                return match.Success ? match.Value : this._videoHeight;
            }
            set
            {
                this._videoHeight = value;
                this.OnPropertyChanged( "VideoHeight" );
            }
        }

        public string VideoDisplayAspectRatio
        {
            get { return this._videoDisplayaspectratio; }
            set
            {
                this._videoDisplayaspectratio = value;
                this.OnPropertyChanged( "VideoDisplayAspectRatio" );
            }
        }

        public string VideoFrameRateMode
        {
            get { return this._videoFrameratemode; }
            set
            {
                this._videoFrameratemode = value;
                this.OnPropertyChanged( "VideoFrameRateMode" );
            }
        }

        public string VideoFrameRate
        {
            get { return this._videoFramerate; }
            set
            {
                this._videoFramerate = value;
                this.OnPropertyChanged( "VideoFrameRate" );
            }
        }

        public string VideoColorSpace
        {
            get { return this._videoColorspace; }
            set
            {
                this._videoColorspace = value;
                this.OnPropertyChanged( "VideoColorSpace" );
            }
        }

        public string VideoChromaSubSampling
        {
            get { return this._videoChromasubsampling; }
            set
            {
                this._videoChromasubsampling = value;
                this.OnPropertyChanged( "VideoChromaSubSampling" );
            }
        }

        public string VideoBitDepth
        {
            get { return this._videoBitdepth; }
            set
            {
                this._videoBitdepth = value;
                this.OnPropertyChanged( "VideoBitDepth" );
            }
        }

        public string VideoCompressionMode
        {
            get { return this._videoCompressionMode; }
            set
            {
                this._videoCompressionMode = value;
                this.OnPropertyChanged( "VideoCompressionMode" );
            }
        }

        public string VideoScanType
        {
            get { return this._videoScantype; }
            set
            {
                this._videoScantype = value;
                this.OnPropertyChanged( "VideoScanType" );
            }
        }

        public string VideoBitsPixelFrame
        {
            get { return this._videoBitsPixelFrame; }
            set
            {
                this._videoBitsPixelFrame = value;
                this.OnPropertyChanged( "VideoBitsPixelFrame" );
            }
        }

        public string VideoStreamSize
        {
            get { return this._videoStreamsize; }
            set
            {
                this._videoStreamsize = value;
                this.OnPropertyChanged( "VideoStreamSize" );
            }
        }

        public string VideoTimeCodeOfFirstFrame
        {
            get { return this._videoTimeCodeOfFirstFrame; }
            set
            {
                this._videoTimeCodeOfFirstFrame = value;
                this.OnPropertyChanged( "VideoTimeCodeOfFirstFrame" );
            }
        }

        public string VideoCodeSource
        {
            get { return this._videoCodecSource; }
            set
            {
                this._videoCodecSource = value;
                this.OnPropertyChanged( "VideoCodeSource" );
            }
        }

        public string VideoWritingLibrary
        {
            get { return this._videoWritinglibrary; }
            set
            {
                this._videoWritinglibrary = value;
                this.OnPropertyChanged( "VideoWritingLibrary" );
            }
        }

        public string VideoEncodingSettings
        {
            get { return this._videoEncodingsettings; }
            set
            {
                this._videoEncodingsettings = value;
                this.OnPropertyChanged( "VideoEncodingSettings" );
            }
        }

        public string VideoLanguage
        {
            get { return this._videoLanguage; }
            set
            {
                this._videoLanguage = value;
                this.OnPropertyChanged( "VideoLanguage" );
            }
        }

        public string VideoDefault
        {
            get { return this._videoDefault; }
            set
            {
                this._videoDefault = value;
                this.OnPropertyChanged( "VideoDefault" );
            }
        }

        public string VideoForced
        {
            get { return this._videoForced; }
            set
            {
                this._videoForced = value;
                this.OnPropertyChanged( "VideoForced" );
            }
        }

        public string VideoColorRange
        {
            get { return this._videoColorrange; }
            set
            {
                this._videoColorrange = value;
                this.OnPropertyChanged( "VideoColorRange" );
            }
        }

        public string VideoMatrixCoefficients
        {
            get { return this._videoMatrixcoefficients; }
            set
            {
                this._videoMatrixcoefficients = value;
                this.OnPropertyChanged( "VideoMatrixCoefficients" );
            }
        }

        public string VideoGopOpenClosed
        {
            get { return this._videoGopOpenClosed; }
            set
            {
                this._videoGopOpenClosed = value;
                this.OnPropertyChanged( "VideoGopOpenClosed" );
            }
        }

        public string VideoGopOpenClosedOfFirstFrame
        {
            get { return this._videoGopOpenClosedOfFirstFrame; }
            set
            {
                this._videoGopOpenClosedOfFirstFrame = value;
                this.OnPropertyChanged( "VideoGopOpenClosedOfFirstFrame" );
            }
        }

        public string VideoMaximumBitRate
        {
            get { return this._videoMaximumBitRate; }
            set
            {
                this._videoMaximumBitRate = value;
                this.OnPropertyChanged( "VideoMaximumBitRate" );
            }
        }

        public string VideoNominalBitRate
        {
            get { return this._videoNominalBitRate; }
            set
            {
                this._videoNominalBitRate = value;
                this.OnPropertyChanged( "VideoNominalBitRate" );
            }
        }

        public string VideoEncodedDate
        {
            get { return this._videoEncodedDate; }
            set
            {
                this._videoEncodedDate = value;
                this.OnPropertyChanged( "VideoEncodedDate" );
            }
        }

        public string VideoTaggedDate
        {
            get { return this._videoTaggedDate; }
            set
            {
                this._videoTaggedDate = value;
                this.OnPropertyChanged( "VideoTaggedDate" );
            }
        }

        public string VideoFormatSettingsPictureStructure
        {
            get { return this._videoFormatSettingsPictureStructure; }
            set
            {
                this._videoFormatSettingsPictureStructure = value;
                this.OnPropertyChanged( "VideoFormatSettingsPictureStructure" );
            }
        }

        public string VideoBitRateMode
        {
            get { return this._videoBitRateMode; }
            set
            {
                this._videoBitRateMode = value;
                this.OnPropertyChanged( "VideoBitRateMode" );
            }
        }

        public string VideoScanOrder
        {
            get { return this._videoScanOrder; }
            set
            {
                this._videoScanOrder = value;
                this.OnPropertyChanged( "VideoScanOrder" );
            }
        }

        public string AudioId
        {
            get { return this._audioId; }
            set
            {
                this._audioId = value;
                this.OnPropertyChanged( "AudioId" );
            }
        }

        public string AudioFormat
        {
            get { return this._audioFormat; }
            set
            {
                this._audioFormat = value;
                this.OnPropertyChanged( "AudioFormat" );
            }
        }

        public string AudioFormatVersion
        {
            get { return this._audioFormatversion; }
            set
            {
                this._audioFormatversion = value;
                this.OnPropertyChanged( "AudioFormatVersion" );
            }
        }

        public string AudioFormatInfo
        {
            get { return this._audioFormatInfo; }
            set
            {
                this._audioFormatInfo = value;
                this.OnPropertyChanged( "AudioFormatInfo" );
            }
        }

        public string AudioFormatProfile
        {
            get { return this._audioFormatprofile; }
            set
            {
                this._audioFormatprofile = value;
                this.OnPropertyChanged( "AudioFormatProfile" );
            }
        }

        public string AudioMode
        {
            get { return this._audioMode; }
            set
            {
                this._audioMode = value;
                this.OnPropertyChanged( "AudioMode" );
            }
        }

        public string AudioModeExtension
        {
            get { return this._audioModeextension; }
            set
            {
                this._audioModeextension = value;
                this.OnPropertyChanged( "AudioModeExtension" );
            }
        }

        public string AudioFormatSettingEndianness
        {
            get { return this._audioFormatsettingEndianness; }
            set
            {
                this._audioFormatsettingEndianness = value;
                this.OnPropertyChanged( "AudioFormatSettingEndianness" );
            }
        }

        public string AudioNominalBitRate
        {
            get { return this._audioNominalbitrate; }
            set
            {
                this._audioNominalbitrate = value;
                this.OnPropertyChanged( "AudioNominalBitRate" );
            }
        }

        public string AudioMinimumBitRate
        {
            get { return this._audioMinimumBitRate; }
            set
            {
                this._audioMinimumBitRate = value;
                this.OnPropertyChanged( "AudioMinimumBitRate" );
            }
        }

        public string AudioCodecId
        {
            get { return this._audioCodecId; }
            set
            {
                this._audioCodecId = value;
                this.OnPropertyChanged( "AudioCodecId" );
            }
        }

        public string AudioCodecIdHint
        {
            get { return this._audioCodecIdHint; }
            set
            {
                this._audioCodecIdHint = value;
                this.OnPropertyChanged( "AudioCodecIdHint" );
            }
        }

        public string AudioDuration
        {
            get { return this._audioDuration; }
            set
            {
                this._audioDuration = value;
                this.OnPropertyChanged( "AudioDuration" );
            }
        }

        public string AudioSourceDuration
        {
            get { return this._audioSourceDuration; }
            set
            {
                this._audioSourceDuration = value;
                this.OnPropertyChanged( "AudioSourceDuration" );
            }
        }

        public string AudioBitRateMode
        {
            get { return this._audioBitratemode; }
            set
            {
                this._audioBitratemode = value;
                this.OnPropertyChanged( "AudioBitRateMode" );
            }
        }

        public string AudioBitRate
        {
            get { return this._audioBitrate; }
            set
            {
                this._audioBitrate = value;
                this.OnPropertyChanged( "AudioBitRate" );
            }
        }

        public string AudioChannel
        {
            get { return this._audioChannel; }
            set
            {
                this._audioChannel = value;
                this.OnPropertyChanged( "AudioChannel" );
            }
        }

        public string AudioChannelPositions
        {
            get { return this._audioChannelPositions; }
            set
            {
                this._audioChannelPositions = value;
                this.OnPropertyChanged( "AudioChannelPositions" );
            }
        }

        public string AudioSamplingRate
        {
            get { return this._audioSamplingRate; }
            set
            {
                this._audioSamplingRate = value;
                this.OnPropertyChanged( "AudioSamplingRate" );
            }
        }

        public string AudioCompressionMode
        {
            get { return this._audioCompressionmode; }
            set
            {
                this._audioCompressionmode = value;
                this.OnPropertyChanged( "AudioCompressionMode" );
            }
        }

        public string AudioStreamSize
        {
            get { return this._audioStreamSize; }
            set
            {
                this._audioStreamSize = value;
                this.OnPropertyChanged( "AudioStreamSize" );
            }
        }

        public string AudioDefault
        {
            get { return this._audioDefault; }
            set
            {
                this._audioDefault = value;
                this.OnPropertyChanged( "AudioDefault" );
            }
        }

        public string AudioForced
        {
            get { return this._audioForced; }
            set
            {
                this._audioForced = value;
                this.OnPropertyChanged( "AudioForced" );
            }
        }

        public string AudioLanguage
        {
            get { return this._audioLanguage; }
            set
            {
                this._audioLanguage = value;
                this.OnPropertyChanged( "AudioLanguage" );
            }
        }

        public string AudioAlignment
        {
            get { return this._audioAlignment; }
            set
            {
                this._audioAlignment = value;
                this.OnPropertyChanged( "AudioAlignment" );
            }
        }

        public string AudioInterleaveDuration
        {
            get { return this._audioInterleaveduration; }
            set
            {
                this._audioInterleaveduration = value;
                this.OnPropertyChanged( "AudioInterleaveDuration" );
            }
        }

        public string AudioInterleavePreloadDuration
        {
            get { return this._audioInterleavepreloadduration; }
            set
            {
                this._audioInterleavepreloadduration = value;
                this.OnPropertyChanged( "AudioInterleavePreloadDuration" );
            }
        }

        public string AudioWritingLibrary
        {
            get { return this._audioWritinglibrary; }
            set
            {
                this._audioWritinglibrary = value;
                this.OnPropertyChanged( "AudioWritingLibrary" );
            }
        }

        public string AudioEncodingSettings
        {
            get { return this._audioEncodingsettings; }
            set
            {
                this._audioEncodingsettings = value;
                this.OnPropertyChanged( "AudioEncodingSettings" );
            }
        }

        public string AudioDelayRelativeToVideo
        {
            get { return this._audioDelayRelativeToVideo; }
            set
            {
                this._audioDelayRelativeToVideo = value;
                this.OnPropertyChanged( "AudioDelayRelativeToVideo" );
            }
        }

        public string AudioMaximumBitRate
        {
            get { return this._audioMaximumBitRate; }
            set
            {
                this._audioMaximumBitRate = value;
                this.OnPropertyChanged( "AudioMaximumBitRate" );
            }
        }

        public string AudioEncodedDate
        {
            get { return this._audioEncodedDate; }
            set
            {
                this._audioEncodedDate = value;
                this.OnPropertyChanged( "AudioEncodedDate" );
            }
        }

        public string AudioTaggedDate
        {
            get { return this._audioTaggedDate; }
            set
            {
                this._audioTaggedDate = value;
                this.OnPropertyChanged( "AudioTaggedDate" );
            }
        }

        public string TextId
        {
            get { return this._textId; }
            set
            {
                this._textId = value;
                this.OnPropertyChanged( "TextId" );
            }
        }

        public string TextFormat
        {
            get { return this._textFormat; }
            set
            {
                this._textFormat = value;
                this.OnPropertyChanged( "TextFormat" );
            }
        }

        public string TextCodecId
        {
            get { return this._textCodecId; }
            set
            {
                this._textCodecId = value;
                this.OnPropertyChanged( "TextCodecId" );
            }
        }

        public string TextCodecIdInfo
        {
            get { return this._textCodecIdInfo; }
            set
            {
                this._textCodecIdInfo = value;
                this.OnPropertyChanged( "TextCodecIdInfo" );
            }
        }

        public string TextTitle
        {
            get { return this._textTitle; }
            set
            {
                this._textTitle = value;
                this.OnPropertyChanged( "TextTitle" );
            }
        }

        public string TextLanguage
        {
            get { return this._textLanguage; }
            set
            {
                this._textLanguage = value;
                this.OnPropertyChanged( "TextLanguage" );
            }
        }

        public string TextDefault
        {
            get { return this._textDefault; }
            set
            {
                this._textDefault = value;
                this.OnPropertyChanged( "TextDefault" );
            }
        }

        public string TextForced
        {
            get { return this._textForced; }
            set
            {
                this._textForced = value;
                this.OnPropertyChanged( "TextForced" );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void parse_media_info( string mediainfo )
        {
            var lines = mediainfo.Split( '\n' );

            var section = 0;

            foreach ( var trimmedline in lines.Select( line => line.Trim() ) )
            {
                if ( String.Compare( trimmedline, "", StringComparison.Ordinal ) == 0 )
                {
                    continue;
                }

                if ( String.Compare( trimmedline, "General", StringComparison.Ordinal ) == 0 )
                {
                    section++;
                    continue;
                }

                if ( String.Compare( trimmedline, "Video", StringComparison.Ordinal ) == 0 )
                {
                    section++;
                    continue;
                }

                if ( String.Compare( trimmedline, "Audio", StringComparison.Ordinal ) == 0 )
                {
                    section++;
                    continue;
                }

                if ( String.Compare( trimmedline, "Text", StringComparison.Ordinal ) == 0 )
                {
                    section++;
                    continue;
                }

                var parts = trimmedline.Split( new[] {':'}, 2 );

                if ( parts.Length < 2 )
                {
                    continue;
                }

                if ( section == 1 )
                {
                    this.parse_section( section, parts[ 0 ].Trim(), parts[ 1 ].Trim(), General );
                }
                if ( section == 2 )
                {
                    this.parse_section( section, parts[ 0 ].Trim(), parts[ 1 ].Trim(), Video );
                }
                if ( section == 3 )
                {
                    this.parse_section( section, parts[ 0 ].Trim(), parts[ 1 ].Trim(), Audio );
                }
                if ( section == 4 )
                {
                    this.parse_section( section, parts[ 0 ].Trim(), parts[ 1 ].Trim(), Text );
                }
            }
        }

        private void parse_section( int section, string tkey, string tvalue, IReadOnlyDictionary< string, string > dict )
        {
            var property = get_property( section, tkey, dict );

            if ( property == null )
            {
                return;
            }

            var mediaFields = typeof (MediaInfo).GetProperties();

            foreach ( var field in mediaFields.Where( field => String.Compare( field.Name, property, StringComparison.Ordinal ) == 0 ) )
            {
                field.SetValue( this, Convert.ChangeType( tvalue, field.PropertyType ), null );
                break;
            }
        }

        private static string get_property( int section, string key, IReadOnlyDictionary< string, string > dict )
        {
            if ( dict.Keys.Contains( key ) )
            {
                return dict[ key ];
            }

            if ( IgnoredTags.Contains( key ) )
            {
                return null;
            }

            return null;
        }

        protected void OnPropertyChanged( string propertyName )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}