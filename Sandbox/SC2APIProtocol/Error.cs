// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: error.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace SC2APIProtocol {

  /// <summary>Holder for reflection information generated from error.proto</summary>
  public static partial class ErrorReflection {

    #region Descriptor
    /// <summary>File descriptor for error.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ErrorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgtlcnJvci5wcm90bxIOU0MyQVBJUHJvdG9jb2wqxS0KDEFjdGlvblJlc3Vs",
            "dBIbChdVbnNwZWNpZmllZEFjdGlvblJlc3VsdBAAEgsKB1N1Y2Nlc3MQARIQ",
            "CgxOb3RTdXBwb3J0ZWQQAhIJCgVFcnJvchADEhYKEkNhbnRRdWV1ZVRoYXRP",
            "cmRlchAEEgkKBVJldHJ5EAUSDAoIQ29vbGRvd24QBhIPCgtRdWV1ZUlzRnVs",
            "bBAHEhQKEFJhbGx5UXVldWVJc0Z1bGwQCBIVChFOb3RFbm91Z2hNaW5lcmFs",
            "cxAJEhQKEE5vdEVub3VnaFZlc3BlbmUQChIWChJOb3RFbm91Z2hUZXJyYXpp",
            "bmUQCxITCg9Ob3RFbm91Z2hDdXN0b20QDBIRCg1Ob3RFbm91Z2hGb29kEA0S",
            "FwoTRm9vZFVzYWdlSW1wb3NzaWJsZRAOEhEKDU5vdEVub3VnaExpZmUQDxIU",
            "ChBOb3RFbm91Z2hTaGllbGRzEBASEwoPTm90RW5vdWdoRW5lcmd5EBESEgoO",
            "TGlmZVN1cHByZXNzZWQQEhIVChFTaGllbGRzU3VwcHJlc3NlZBATEhQKEEVu",
            "ZXJneVN1cHByZXNzZWQQFBIUChBOb3RFbm91Z2hDaGFyZ2VzEBUSFgoSQ2Fu",
            "dEFkZE1vcmVDaGFyZ2VzEBYSEwoPVG9vTXVjaE1pbmVyYWxzEBcSEgoOVG9v",
            "TXVjaFZlc3BlbmUQGBIUChBUb29NdWNoVGVycmF6aW5lEBkSEQoNVG9vTXVj",
            "aEN1c3RvbRAaEg8KC1Rvb011Y2hGb29kEBsSDwoLVG9vTXVjaExpZmUQHBIS",
            "Cg5Ub29NdWNoU2hpZWxkcxAdEhEKDVRvb011Y2hFbmVyZ3kQHhIaChZNdXN0",
            "VGFyZ2V0VW5pdFdpdGhMaWZlEB8SHQoZTXVzdFRhcmdldFVuaXRXaXRoU2hp",
            "ZWxkcxAgEhwKGE11c3RUYXJnZXRVbml0V2l0aEVuZXJneRAhEg0KCUNhbnRU",
            "cmFkZRAiEg0KCUNhbnRTcGVuZBAjEhYKEkNhbnRUYXJnZXRUaGF0VW5pdBAk",
            "EhcKE0NvdWxkbnRBbGxvY2F0ZVVuaXQQJRIQCgxVbml0Q2FudE1vdmUQJhIe",
            "ChpUcmFuc3BvcnRJc0hvbGRpbmdQb3NpdGlvbhAnEh8KG0J1aWxkVGVjaFJl",
            "cXVpcmVtZW50c05vdE1ldBAoEh0KGUNhbnRGaW5kUGxhY2VtZW50TG9jYXRp",
            "b24QKRITCg9DYW50QnVpbGRPblRoYXQQKhIeChpDYW50QnVpbGRUb29DbG9z",
            "ZVRvRHJvcE9mZhArEhwKGENhbnRCdWlsZExvY2F0aW9uSW52YWxpZBAsEhgK",
            "FENhbnRTZWVCdWlsZExvY2F0aW9uEC0SIgoeQ2FudEJ1aWxkVG9vQ2xvc2VU",
            "b0NyZWVwU291cmNlEC4SIAocQ2FudEJ1aWxkVG9vQ2xvc2VUb1Jlc291cmNl",
            "cxAvEhwKGENhbnRCdWlsZFRvb0ZhckZyb21XYXRlchAwEiIKHkNhbnRCdWls",
            "ZFRvb0ZhckZyb21DcmVlcFNvdXJjZRAxEicKI0NhbnRCdWlsZFRvb0ZhckZy",
            "b21CdWlsZFBvd2VyU291cmNlEDISGwoXQ2FudEJ1aWxkT25EZW5zZVRlcnJh",
            "aW4QMxInCiNDYW50VHJhaW5Ub29GYXJGcm9tVHJhaW5Qb3dlclNvdXJjZRA0",
            "EhsKF0NhbnRMYW5kTG9jYXRpb25JbnZhbGlkEDUSFwoTQ2FudFNlZUxhbmRM",
            "b2NhdGlvbhA2EiEKHUNhbnRMYW5kVG9vQ2xvc2VUb0NyZWVwU291cmNlEDcS",
            "HwobQ2FudExhbmRUb29DbG9zZVRvUmVzb3VyY2VzEDgSGwoXQ2FudExhbmRU",
            "b29GYXJGcm9tV2F0ZXIQORIhCh1DYW50TGFuZFRvb0ZhckZyb21DcmVlcFNv",
            "dXJjZRA6EiYKIkNhbnRMYW5kVG9vRmFyRnJvbUJ1aWxkUG93ZXJTb3VyY2UQ",
            "OxImCiJDYW50TGFuZFRvb0ZhckZyb21UcmFpblBvd2VyU291cmNlEDwSGgoW",
            "Q2FudExhbmRPbkRlbnNlVGVycmFpbhA9EhsKF0FkZE9uVG9vRmFyRnJvbUJ1",
            "aWxkaW5nED4SGgoWTXVzdEJ1aWxkUmVmaW5lcnlGaXJzdBA/Eh8KG0J1aWxk",
            "aW5nSXNVbmRlckNvbnN0cnVjdGlvbhBAEhMKD0NhbnRGaW5kRHJvcE9mZhBB",
            "Eh0KGUNhbnRMb2FkT3RoZXJQbGF5ZXJzVW5pdHMQQhIbChdOb3RFbm91Z2hS",
            "b29tVG9Mb2FkVW5pdBBDEhgKFENhbnRVbmxvYWRVbml0c1RoZXJlEEQSGAoU",
            "Q2FudFdhcnBJblVuaXRzVGhlcmUQRRIZChVDYW50TG9hZEltbW9iaWxlVW5p",
            "dHMQRhIdChlDYW50UmVjaGFyZ2VJbW1vYmlsZVVuaXRzEEcSJgoiQ2FudFJl",
            "Y2hhcmdlVW5kZXJDb25zdHJ1Y3Rpb25Vbml0cxBIEhQKEENhbnRMb2FkVGhh",
            "dFVuaXQQSRITCg9Ob0NhcmdvVG9VbmxvYWQQShIZChVMb2FkQWxsTm9UYXJn",
            "ZXRzRm91bmQQSxIUChBOb3RXaGlsZU9jY3VwaWVkEEwSGQoVQ2FudEF0dGFj",
            "a1dpdGhvdXRBbW1vEE0SFwoTQ2FudEhvbGRBbnlNb3JlQW1tbxBOEhoKFlRl",
            "Y2hSZXF1aXJlbWVudHNOb3RNZXQQTxIZChVNdXN0TG9ja2Rvd25Vbml0Rmly",
            "c3QQUBISCg5NdXN0VGFyZ2V0VW5pdBBREhcKE011c3RUYXJnZXRJbnZlbnRv",
            "cnkQUhIZChVNdXN0VGFyZ2V0VmlzaWJsZVVuaXQQUxIdChlNdXN0VGFyZ2V0",
            "VmlzaWJsZUxvY2F0aW9uEFQSHgoaTXVzdFRhcmdldFdhbGthYmxlTG9jYXRp",
            "b24QVRIaChZNdXN0VGFyZ2V0UGF3bmFibGVVbml0EFYSGgoWWW91Q2FudENv",
            "bnRyb2xUaGF0VW5pdBBXEiIKHllvdUNhbnRJc3N1ZUNvbW1hbmRzVG9UaGF0",
            "VW5pdBBYEhcKE011c3RUYXJnZXRSZXNvdXJjZXMQWRIWChJSZXF1aXJlc0hl",
            "YWxUYXJnZXQQWhIYChRSZXF1aXJlc1JlcGFpclRhcmdldBBbEhEKDU5vSXRl",
            "bXNUb0Ryb3AQXBIYChRDYW50SG9sZEFueU1vcmVJdGVtcxBdEhAKDENhbnRI",
            "b2xkVGhhdBBeEhgKFFRhcmdldEhhc05vSW52ZW50b3J5EF8SFAoQQ2FudERy",
            "b3BUaGlzSXRlbRBgEhQKEENhbnRNb3ZlVGhpc0l0ZW0QYRIUChBDYW50UGF3",
            "blRoaXNVbml0EGISFAoQTXVzdFRhcmdldENhc3RlchBjEhQKEENhbnRUYXJn",
            "ZXRDYXN0ZXIQZBITCg9NdXN0VGFyZ2V0T3V0ZXIQZRITCg9DYW50VGFyZ2V0",
            "T3V0ZXIQZhIaChZNdXN0VGFyZ2V0WW91ck93blVuaXRzEGcSGgoWQ2FudFRh",
            "cmdldFlvdXJPd25Vbml0cxBoEhsKF011c3RUYXJnZXRGcmllbmRseVVuaXRz",
            "EGkSGwoXQ2FudFRhcmdldEZyaWVuZGx5VW5pdHMQahIaChZNdXN0VGFyZ2V0",
            "TmV1dHJhbFVuaXRzEGsSGgoWQ2FudFRhcmdldE5ldXRyYWxVbml0cxBsEhgK",
            "FE11c3RUYXJnZXRFbmVteVVuaXRzEG0SGAoUQ2FudFRhcmdldEVuZW15VW5p",
            "dHMQbhIWChJNdXN0VGFyZ2V0QWlyVW5pdHMQbxIWChJDYW50VGFyZ2V0QWly",
            "VW5pdHMQcBIZChVNdXN0VGFyZ2V0R3JvdW5kVW5pdHMQcRIZChVDYW50VGFy",
            "Z2V0R3JvdW5kVW5pdHMQchIYChRNdXN0VGFyZ2V0U3RydWN0dXJlcxBzEhgK",
            "FENhbnRUYXJnZXRTdHJ1Y3R1cmVzEHQSGAoUTXVzdFRhcmdldExpZ2h0VW5p",
            "dHMQdRIYChRDYW50VGFyZ2V0TGlnaHRVbml0cxB2EhoKFk11c3RUYXJnZXRB",
            "cm1vcmVkVW5pdHMQdxIaChZDYW50VGFyZ2V0QXJtb3JlZFVuaXRzEHgSHQoZ",
            "TXVzdFRhcmdldEJpb2xvZ2ljYWxVbml0cxB5Eh0KGUNhbnRUYXJnZXRCaW9s",
            "b2dpY2FsVW5pdHMQehIZChVNdXN0VGFyZ2V0SGVyb2ljVW5pdHMQexIZChVD",
            "YW50VGFyZ2V0SGVyb2ljVW5pdHMQfBIaChZNdXN0VGFyZ2V0Um9ib3RpY1Vu",
            "aXRzEH0SGgoWQ2FudFRhcmdldFJvYm90aWNVbml0cxB+Eh0KGU11c3RUYXJn",
            "ZXRNZWNoYW5pY2FsVW5pdHMQfxIeChlDYW50VGFyZ2V0TWVjaGFuaWNhbFVu",
            "aXRzEIABEhsKFk11c3RUYXJnZXRQc2lvbmljVW5pdHMQgQESGwoWQ2FudFRh",
            "cmdldFBzaW9uaWNVbml0cxCCARIbChZNdXN0VGFyZ2V0TWFzc2l2ZVVuaXRz",
            "EIMBEhsKFkNhbnRUYXJnZXRNYXNzaXZlVW5pdHMQhAESFgoRTXVzdFRhcmdl",
            "dE1pc3NpbGUQhQESFgoRQ2FudFRhcmdldE1pc3NpbGUQhgESGgoVTXVzdFRh",
            "cmdldFdvcmtlclVuaXRzEIcBEhoKFUNhbnRUYXJnZXRXb3JrZXJVbml0cxCI",
            "ARIhChxNdXN0VGFyZ2V0RW5lcmd5Q2FwYWJsZVVuaXRzEIkBEiEKHENhbnRU",
            "YXJnZXRFbmVyZ3lDYXBhYmxlVW5pdHMQigESIQocTXVzdFRhcmdldFNoaWVs",
            "ZENhcGFibGVVbml0cxCLARIhChxDYW50VGFyZ2V0U2hpZWxkQ2FwYWJsZVVu",
            "aXRzEIwBEhUKEE11c3RUYXJnZXRGbHllcnMQjQESFQoQQ2FudFRhcmdldEZs",
            "eWVycxCOARIaChVNdXN0VGFyZ2V0QnVyaWVkVW5pdHMQjwESGgoVQ2FudFRh",
            "cmdldEJ1cmllZFVuaXRzEJABEhsKFk11c3RUYXJnZXRDbG9ha2VkVW5pdHMQ",
            "kQESGwoWQ2FudFRhcmdldENsb2FrZWRVbml0cxCSARIiCh1NdXN0VGFyZ2V0",
            "VW5pdHNJbkFTdGFzaXNGaWVsZBCTARIiCh1DYW50VGFyZ2V0VW5pdHNJbkFT",
            "dGFzaXNGaWVsZBCUARIlCiBNdXN0VGFyZ2V0VW5kZXJDb25zdHJ1Y3Rpb25V",
            "bml0cxCVARIlCiBDYW50VGFyZ2V0VW5kZXJDb25zdHJ1Y3Rpb25Vbml0cxCW",
            "ARIYChNNdXN0VGFyZ2V0RGVhZFVuaXRzEJcBEhgKE0NhbnRUYXJnZXREZWFk",
            "VW5pdHMQmAESHQoYTXVzdFRhcmdldFJldml2YWJsZVVuaXRzEJkBEh0KGENh",
            "bnRUYXJnZXRSZXZpdmFibGVVbml0cxCaARIaChVNdXN0VGFyZ2V0SGlkZGVu",
            "VW5pdHMQmwESGgoVQ2FudFRhcmdldEhpZGRlblVuaXRzEJwBEiIKHUNhbnRS",
            "ZWNoYXJnZU90aGVyUGxheWVyc1VuaXRzEJ0BEh0KGE11c3RUYXJnZXRIYWxs",
            "dWNpbmF0aW9ucxCeARIdChhDYW50VGFyZ2V0SGFsbHVjaW5hdGlvbnMQnwES",
            "IAobTXVzdFRhcmdldEludnVsbmVyYWJsZVVuaXRzEKABEiAKG0NhbnRUYXJn",
            "ZXRJbnZ1bG5lcmFibGVVbml0cxChARIcChdNdXN0VGFyZ2V0RGV0ZWN0ZWRV",
            "bml0cxCiARIcChdDYW50VGFyZ2V0RGV0ZWN0ZWRVbml0cxCjARIdChhDYW50",
            "VGFyZ2V0VW5pdFdpdGhFbmVyZ3kQpAESHgoZQ2FudFRhcmdldFVuaXRXaXRo",
            "U2hpZWxkcxClARIhChxNdXN0VGFyZ2V0VW5jb21tYW5kYWJsZVVuaXRzEKYB",
            "EiEKHENhbnRUYXJnZXRVbmNvbW1hbmRhYmxlVW5pdHMQpwESIQocTXVzdFRh",
            "cmdldFByZXZlbnREZWZlYXRVbml0cxCoARIhChxDYW50VGFyZ2V0UHJldmVu",
            "dERlZmVhdFVuaXRzEKkBEiEKHE11c3RUYXJnZXRQcmV2ZW50UmV2ZWFsVW5p",
            "dHMQqgESIQocQ2FudFRhcmdldFByZXZlbnRSZXZlYWxVbml0cxCrARIbChZN",
            "dXN0VGFyZ2V0UGFzc2l2ZVVuaXRzEKwBEhsKFkNhbnRUYXJnZXRQYXNzaXZl",
            "VW5pdHMQrQESGwoWTXVzdFRhcmdldFN0dW5uZWRVbml0cxCuARIbChZDYW50",
            "VGFyZ2V0U3R1bm5lZFVuaXRzEK8BEhwKF011c3RUYXJnZXRTdW1tb25lZFVu",
            "aXRzELABEhwKF0NhbnRUYXJnZXRTdW1tb25lZFVuaXRzELEBEhQKD011c3RU",
            "YXJnZXRVc2VyMRCyARIUCg9DYW50VGFyZ2V0VXNlcjEQswESHwoaTXVzdFRh",
            "cmdldFVuc3RvcHBhYmxlVW5pdHMQtAESHwoaQ2FudFRhcmdldFVuc3RvcHBh",
            "YmxlVW5pdHMQtQESHQoYTXVzdFRhcmdldFJlc2lzdGFudFVuaXRzELYBEh0K",
            "GENhbnRUYXJnZXRSZXNpc3RhbnRVbml0cxC3ARIZChRNdXN0VGFyZ2V0RGF6",
            "ZWRVbml0cxC4ARIZChRDYW50VGFyZ2V0RGF6ZWRVbml0cxC5ARIRCgxDYW50",
            "TG9ja2Rvd24QugESFAoPQ2FudE1pbmRDb250cm9sELsBEhwKF011c3RUYXJn",
            "ZXREZXN0cnVjdGlibGVzELwBEhwKF0NhbnRUYXJnZXREZXN0cnVjdGlibGVz",
            "EL0BEhQKD011c3RUYXJnZXRJdGVtcxC+ARIUCg9DYW50VGFyZ2V0SXRlbXMQ",
            "vwESGAoTTm9DYWxsZG93bkF2YWlsYWJsZRDAARIVChBXYXlwb2ludExpc3RG",
            "dWxsEMEBEhMKDk11c3RUYXJnZXRSYWNlEMIBEhMKDkNhbnRUYXJnZXRSYWNl",
            "EMMBEhsKFk11c3RUYXJnZXRTaW1pbGFyVW5pdHMQxAESGwoWQ2FudFRhcmdl",
            "dFNpbWlsYXJVbml0cxDFARIaChVDYW50RmluZEVub3VnaFRhcmdldHMQxgES",
            "GQoUQWxyZWFkeVNwYXduaW5nTGFydmEQxwESIQocQ2FudFRhcmdldEV4aGF1",
            "c3RlZFJlc291cmNlcxDIARITCg5DYW50VXNlTWluaW1hcBDJARIVChBDYW50",
            "VXNlSW5mb1BhbmVsEMoBEhUKEE9yZGVyUXVldWVJc0Z1bGwQywESHAoXQ2Fu",
            "dEhhcnZlc3RUaGF0UmVzb3VyY2UQzAESGgoVSGFydmVzdGVyc05vdFJlcXVp",
            "cmVkEM0BEhQKD0FscmVhZHlUYXJnZXRlZBDOARIeChlDYW50QXR0YWNrV2Vh",
            "cG9uc0Rpc2FibGVkEM8BEhcKEkNvdWxkbnRSZWFjaFRhcmdldBDQARIXChJU",
            "YXJnZXRJc091dE9mUmFuZ2UQ0QESFQoQVGFyZ2V0SXNUb29DbG9zZRDSARIV",
            "ChBUYXJnZXRJc091dE9mQXJjENMBEh0KGENhbnRGaW5kVGVsZXBvcnRMb2Nh",
            "dGlvbhDUARIVChBJbnZhbGlkSXRlbUNsYXNzENUBEhgKE0NhbnRGaW5kQ2Fu",
            "Y2VsT3JkZXIQ1gFiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::SC2APIProtocol.ActionResult), }, null));
    }
    #endregion

  }
  #region Enums
  public enum ActionResult {
    [pbr::OriginalName("UnspecifiedActionResult")] UnspecifiedActionResult = 0,
    [pbr::OriginalName("Success")] Success = 1,
    [pbr::OriginalName("NotSupported")] NotSupported = 2,
    [pbr::OriginalName("Error")] Error = 3,
    [pbr::OriginalName("CantQueueThatOrder")] CantQueueThatOrder = 4,
    [pbr::OriginalName("Retry")] Retry = 5,
    [pbr::OriginalName("Cooldown")] Cooldown = 6,
    [pbr::OriginalName("QueueIsFull")] QueueIsFull = 7,
    [pbr::OriginalName("RallyQueueIsFull")] RallyQueueIsFull = 8,
    [pbr::OriginalName("NotEnoughMinerals")] NotEnoughMinerals = 9,
    [pbr::OriginalName("NotEnoughVespene")] NotEnoughVespene = 10,
    [pbr::OriginalName("NotEnoughTerrazine")] NotEnoughTerrazine = 11,
    [pbr::OriginalName("NotEnoughCustom")] NotEnoughCustom = 12,
    [pbr::OriginalName("NotEnoughFood")] NotEnoughFood = 13,
    [pbr::OriginalName("FoodUsageImpossible")] FoodUsageImpossible = 14,
    [pbr::OriginalName("NotEnoughLife")] NotEnoughLife = 15,
    [pbr::OriginalName("NotEnoughShields")] NotEnoughShields = 16,
    [pbr::OriginalName("NotEnoughEnergy")] NotEnoughEnergy = 17,
    [pbr::OriginalName("LifeSuppressed")] LifeSuppressed = 18,
    [pbr::OriginalName("ShieldsSuppressed")] ShieldsSuppressed = 19,
    [pbr::OriginalName("EnergySuppressed")] EnergySuppressed = 20,
    [pbr::OriginalName("NotEnoughCharges")] NotEnoughCharges = 21,
    [pbr::OriginalName("CantAddMoreCharges")] CantAddMoreCharges = 22,
    [pbr::OriginalName("TooMuchMinerals")] TooMuchMinerals = 23,
    [pbr::OriginalName("TooMuchVespene")] TooMuchVespene = 24,
    [pbr::OriginalName("TooMuchTerrazine")] TooMuchTerrazine = 25,
    [pbr::OriginalName("TooMuchCustom")] TooMuchCustom = 26,
    [pbr::OriginalName("TooMuchFood")] TooMuchFood = 27,
    [pbr::OriginalName("TooMuchLife")] TooMuchLife = 28,
    [pbr::OriginalName("TooMuchShields")] TooMuchShields = 29,
    [pbr::OriginalName("TooMuchEnergy")] TooMuchEnergy = 30,
    [pbr::OriginalName("MustTargetUnitWithLife")] MustTargetUnitWithLife = 31,
    [pbr::OriginalName("MustTargetUnitWithShields")] MustTargetUnitWithShields = 32,
    [pbr::OriginalName("MustTargetUnitWithEnergy")] MustTargetUnitWithEnergy = 33,
    [pbr::OriginalName("CantTrade")] CantTrade = 34,
    [pbr::OriginalName("CantSpend")] CantSpend = 35,
    [pbr::OriginalName("CantTargetThatUnit")] CantTargetThatUnit = 36,
    [pbr::OriginalName("CouldntAllocateUnit")] CouldntAllocateUnit = 37,
    [pbr::OriginalName("UnitCantMove")] UnitCantMove = 38,
    [pbr::OriginalName("TransportIsHoldingPosition")] TransportIsHoldingPosition = 39,
    [pbr::OriginalName("BuildTechRequirementsNotMet")] BuildTechRequirementsNotMet = 40,
    [pbr::OriginalName("CantFindPlacementLocation")] CantFindPlacementLocation = 41,
    [pbr::OriginalName("CantBuildOnThat")] CantBuildOnThat = 42,
    [pbr::OriginalName("CantBuildTooCloseToDropOff")] CantBuildTooCloseToDropOff = 43,
    [pbr::OriginalName("CantBuildLocationInvalid")] CantBuildLocationInvalid = 44,
    [pbr::OriginalName("CantSeeBuildLocation")] CantSeeBuildLocation = 45,
    [pbr::OriginalName("CantBuildTooCloseToCreepSource")] CantBuildTooCloseToCreepSource = 46,
    [pbr::OriginalName("CantBuildTooCloseToResources")] CantBuildTooCloseToResources = 47,
    [pbr::OriginalName("CantBuildTooFarFromWater")] CantBuildTooFarFromWater = 48,
    [pbr::OriginalName("CantBuildTooFarFromCreepSource")] CantBuildTooFarFromCreepSource = 49,
    [pbr::OriginalName("CantBuildTooFarFromBuildPowerSource")] CantBuildTooFarFromBuildPowerSource = 50,
    [pbr::OriginalName("CantBuildOnDenseTerrain")] CantBuildOnDenseTerrain = 51,
    [pbr::OriginalName("CantTrainTooFarFromTrainPowerSource")] CantTrainTooFarFromTrainPowerSource = 52,
    [pbr::OriginalName("CantLandLocationInvalid")] CantLandLocationInvalid = 53,
    [pbr::OriginalName("CantSeeLandLocation")] CantSeeLandLocation = 54,
    [pbr::OriginalName("CantLandTooCloseToCreepSource")] CantLandTooCloseToCreepSource = 55,
    [pbr::OriginalName("CantLandTooCloseToResources")] CantLandTooCloseToResources = 56,
    [pbr::OriginalName("CantLandTooFarFromWater")] CantLandTooFarFromWater = 57,
    [pbr::OriginalName("CantLandTooFarFromCreepSource")] CantLandTooFarFromCreepSource = 58,
    [pbr::OriginalName("CantLandTooFarFromBuildPowerSource")] CantLandTooFarFromBuildPowerSource = 59,
    [pbr::OriginalName("CantLandTooFarFromTrainPowerSource")] CantLandTooFarFromTrainPowerSource = 60,
    [pbr::OriginalName("CantLandOnDenseTerrain")] CantLandOnDenseTerrain = 61,
    [pbr::OriginalName("AddOnTooFarFromBuilding")] AddOnTooFarFromBuilding = 62,
    [pbr::OriginalName("MustBuildRefineryFirst")] MustBuildRefineryFirst = 63,
    [pbr::OriginalName("BuildingIsUnderConstruction")] BuildingIsUnderConstruction = 64,
    [pbr::OriginalName("CantFindDropOff")] CantFindDropOff = 65,
    [pbr::OriginalName("CantLoadOtherPlayersUnits")] CantLoadOtherPlayersUnits = 66,
    [pbr::OriginalName("NotEnoughRoomToLoadUnit")] NotEnoughRoomToLoadUnit = 67,
    [pbr::OriginalName("CantUnloadUnitsThere")] CantUnloadUnitsThere = 68,
    [pbr::OriginalName("CantWarpInUnitsThere")] CantWarpInUnitsThere = 69,
    [pbr::OriginalName("CantLoadImmobileUnits")] CantLoadImmobileUnits = 70,
    [pbr::OriginalName("CantRechargeImmobileUnits")] CantRechargeImmobileUnits = 71,
    [pbr::OriginalName("CantRechargeUnderConstructionUnits")] CantRechargeUnderConstructionUnits = 72,
    [pbr::OriginalName("CantLoadThatUnit")] CantLoadThatUnit = 73,
    [pbr::OriginalName("NoCargoToUnload")] NoCargoToUnload = 74,
    [pbr::OriginalName("LoadAllNoTargetsFound")] LoadAllNoTargetsFound = 75,
    [pbr::OriginalName("NotWhileOccupied")] NotWhileOccupied = 76,
    [pbr::OriginalName("CantAttackWithoutAmmo")] CantAttackWithoutAmmo = 77,
    [pbr::OriginalName("CantHoldAnyMoreAmmo")] CantHoldAnyMoreAmmo = 78,
    [pbr::OriginalName("TechRequirementsNotMet")] TechRequirementsNotMet = 79,
    [pbr::OriginalName("MustLockdownUnitFirst")] MustLockdownUnitFirst = 80,
    [pbr::OriginalName("MustTargetUnit")] MustTargetUnit = 81,
    [pbr::OriginalName("MustTargetInventory")] MustTargetInventory = 82,
    [pbr::OriginalName("MustTargetVisibleUnit")] MustTargetVisibleUnit = 83,
    [pbr::OriginalName("MustTargetVisibleLocation")] MustTargetVisibleLocation = 84,
    [pbr::OriginalName("MustTargetWalkableLocation")] MustTargetWalkableLocation = 85,
    [pbr::OriginalName("MustTargetPawnableUnit")] MustTargetPawnableUnit = 86,
    [pbr::OriginalName("YouCantControlThatUnit")] YouCantControlThatUnit = 87,
    [pbr::OriginalName("YouCantIssueCommandsToThatUnit")] YouCantIssueCommandsToThatUnit = 88,
    [pbr::OriginalName("MustTargetResources")] MustTargetResources = 89,
    [pbr::OriginalName("RequiresHealTarget")] RequiresHealTarget = 90,
    [pbr::OriginalName("RequiresRepairTarget")] RequiresRepairTarget = 91,
    [pbr::OriginalName("NoItemsToDrop")] NoItemsToDrop = 92,
    [pbr::OriginalName("CantHoldAnyMoreItems")] CantHoldAnyMoreItems = 93,
    [pbr::OriginalName("CantHoldThat")] CantHoldThat = 94,
    [pbr::OriginalName("TargetHasNoInventory")] TargetHasNoInventory = 95,
    [pbr::OriginalName("CantDropThisItem")] CantDropThisItem = 96,
    [pbr::OriginalName("CantMoveThisItem")] CantMoveThisItem = 97,
    [pbr::OriginalName("CantPawnThisUnit")] CantPawnThisUnit = 98,
    [pbr::OriginalName("MustTargetCaster")] MustTargetCaster = 99,
    [pbr::OriginalName("CantTargetCaster")] CantTargetCaster = 100,
    [pbr::OriginalName("MustTargetOuter")] MustTargetOuter = 101,
    [pbr::OriginalName("CantTargetOuter")] CantTargetOuter = 102,
    [pbr::OriginalName("MustTargetYourOwnUnits")] MustTargetYourOwnUnits = 103,
    [pbr::OriginalName("CantTargetYourOwnUnits")] CantTargetYourOwnUnits = 104,
    [pbr::OriginalName("MustTargetFriendlyUnits")] MustTargetFriendlyUnits = 105,
    [pbr::OriginalName("CantTargetFriendlyUnits")] CantTargetFriendlyUnits = 106,
    [pbr::OriginalName("MustTargetNeutralUnits")] MustTargetNeutralUnits = 107,
    [pbr::OriginalName("CantTargetNeutralUnits")] CantTargetNeutralUnits = 108,
    [pbr::OriginalName("MustTargetEnemyUnits")] MustTargetEnemyUnits = 109,
    [pbr::OriginalName("CantTargetEnemyUnits")] CantTargetEnemyUnits = 110,
    [pbr::OriginalName("MustTargetAirUnits")] MustTargetAirUnits = 111,
    [pbr::OriginalName("CantTargetAirUnits")] CantTargetAirUnits = 112,
    [pbr::OriginalName("MustTargetGroundUnits")] MustTargetGroundUnits = 113,
    [pbr::OriginalName("CantTargetGroundUnits")] CantTargetGroundUnits = 114,
    [pbr::OriginalName("MustTargetStructures")] MustTargetStructures = 115,
    [pbr::OriginalName("CantTargetStructures")] CantTargetStructures = 116,
    [pbr::OriginalName("MustTargetLightUnits")] MustTargetLightUnits = 117,
    [pbr::OriginalName("CantTargetLightUnits")] CantTargetLightUnits = 118,
    [pbr::OriginalName("MustTargetArmoredUnits")] MustTargetArmoredUnits = 119,
    [pbr::OriginalName("CantTargetArmoredUnits")] CantTargetArmoredUnits = 120,
    [pbr::OriginalName("MustTargetBiologicalUnits")] MustTargetBiologicalUnits = 121,
    [pbr::OriginalName("CantTargetBiologicalUnits")] CantTargetBiologicalUnits = 122,
    [pbr::OriginalName("MustTargetHeroicUnits")] MustTargetHeroicUnits = 123,
    [pbr::OriginalName("CantTargetHeroicUnits")] CantTargetHeroicUnits = 124,
    [pbr::OriginalName("MustTargetRoboticUnits")] MustTargetRoboticUnits = 125,
    [pbr::OriginalName("CantTargetRoboticUnits")] CantTargetRoboticUnits = 126,
    [pbr::OriginalName("MustTargetMechanicalUnits")] MustTargetMechanicalUnits = 127,
    [pbr::OriginalName("CantTargetMechanicalUnits")] CantTargetMechanicalUnits = 128,
    [pbr::OriginalName("MustTargetPsionicUnits")] MustTargetPsionicUnits = 129,
    [pbr::OriginalName("CantTargetPsionicUnits")] CantTargetPsionicUnits = 130,
    [pbr::OriginalName("MustTargetMassiveUnits")] MustTargetMassiveUnits = 131,
    [pbr::OriginalName("CantTargetMassiveUnits")] CantTargetMassiveUnits = 132,
    [pbr::OriginalName("MustTargetMissile")] MustTargetMissile = 133,
    [pbr::OriginalName("CantTargetMissile")] CantTargetMissile = 134,
    [pbr::OriginalName("MustTargetWorkerUnits")] MustTargetWorkerUnits = 135,
    [pbr::OriginalName("CantTargetWorkerUnits")] CantTargetWorkerUnits = 136,
    [pbr::OriginalName("MustTargetEnergyCapableUnits")] MustTargetEnergyCapableUnits = 137,
    [pbr::OriginalName("CantTargetEnergyCapableUnits")] CantTargetEnergyCapableUnits = 138,
    [pbr::OriginalName("MustTargetShieldCapableUnits")] MustTargetShieldCapableUnits = 139,
    [pbr::OriginalName("CantTargetShieldCapableUnits")] CantTargetShieldCapableUnits = 140,
    [pbr::OriginalName("MustTargetFlyers")] MustTargetFlyers = 141,
    [pbr::OriginalName("CantTargetFlyers")] CantTargetFlyers = 142,
    [pbr::OriginalName("MustTargetBuriedUnits")] MustTargetBuriedUnits = 143,
    [pbr::OriginalName("CantTargetBuriedUnits")] CantTargetBuriedUnits = 144,
    [pbr::OriginalName("MustTargetCloakedUnits")] MustTargetCloakedUnits = 145,
    [pbr::OriginalName("CantTargetCloakedUnits")] CantTargetCloakedUnits = 146,
    [pbr::OriginalName("MustTargetUnitsInAStasisField")] MustTargetUnitsInAstasisField = 147,
    [pbr::OriginalName("CantTargetUnitsInAStasisField")] CantTargetUnitsInAstasisField = 148,
    [pbr::OriginalName("MustTargetUnderConstructionUnits")] MustTargetUnderConstructionUnits = 149,
    [pbr::OriginalName("CantTargetUnderConstructionUnits")] CantTargetUnderConstructionUnits = 150,
    [pbr::OriginalName("MustTargetDeadUnits")] MustTargetDeadUnits = 151,
    [pbr::OriginalName("CantTargetDeadUnits")] CantTargetDeadUnits = 152,
    [pbr::OriginalName("MustTargetRevivableUnits")] MustTargetRevivableUnits = 153,
    [pbr::OriginalName("CantTargetRevivableUnits")] CantTargetRevivableUnits = 154,
    [pbr::OriginalName("MustTargetHiddenUnits")] MustTargetHiddenUnits = 155,
    [pbr::OriginalName("CantTargetHiddenUnits")] CantTargetHiddenUnits = 156,
    [pbr::OriginalName("CantRechargeOtherPlayersUnits")] CantRechargeOtherPlayersUnits = 157,
    [pbr::OriginalName("MustTargetHallucinations")] MustTargetHallucinations = 158,
    [pbr::OriginalName("CantTargetHallucinations")] CantTargetHallucinations = 159,
    [pbr::OriginalName("MustTargetInvulnerableUnits")] MustTargetInvulnerableUnits = 160,
    [pbr::OriginalName("CantTargetInvulnerableUnits")] CantTargetInvulnerableUnits = 161,
    [pbr::OriginalName("MustTargetDetectedUnits")] MustTargetDetectedUnits = 162,
    [pbr::OriginalName("CantTargetDetectedUnits")] CantTargetDetectedUnits = 163,
    [pbr::OriginalName("CantTargetUnitWithEnergy")] CantTargetUnitWithEnergy = 164,
    [pbr::OriginalName("CantTargetUnitWithShields")] CantTargetUnitWithShields = 165,
    [pbr::OriginalName("MustTargetUncommandableUnits")] MustTargetUncommandableUnits = 166,
    [pbr::OriginalName("CantTargetUncommandableUnits")] CantTargetUncommandableUnits = 167,
    [pbr::OriginalName("MustTargetPreventDefeatUnits")] MustTargetPreventDefeatUnits = 168,
    [pbr::OriginalName("CantTargetPreventDefeatUnits")] CantTargetPreventDefeatUnits = 169,
    [pbr::OriginalName("MustTargetPreventRevealUnits")] MustTargetPreventRevealUnits = 170,
    [pbr::OriginalName("CantTargetPreventRevealUnits")] CantTargetPreventRevealUnits = 171,
    [pbr::OriginalName("MustTargetPassiveUnits")] MustTargetPassiveUnits = 172,
    [pbr::OriginalName("CantTargetPassiveUnits")] CantTargetPassiveUnits = 173,
    [pbr::OriginalName("MustTargetStunnedUnits")] MustTargetStunnedUnits = 174,
    [pbr::OriginalName("CantTargetStunnedUnits")] CantTargetStunnedUnits = 175,
    [pbr::OriginalName("MustTargetSummonedUnits")] MustTargetSummonedUnits = 176,
    [pbr::OriginalName("CantTargetSummonedUnits")] CantTargetSummonedUnits = 177,
    [pbr::OriginalName("MustTargetUser1")] MustTargetUser1 = 178,
    [pbr::OriginalName("CantTargetUser1")] CantTargetUser1 = 179,
    [pbr::OriginalName("MustTargetUnstoppableUnits")] MustTargetUnstoppableUnits = 180,
    [pbr::OriginalName("CantTargetUnstoppableUnits")] CantTargetUnstoppableUnits = 181,
    [pbr::OriginalName("MustTargetResistantUnits")] MustTargetResistantUnits = 182,
    [pbr::OriginalName("CantTargetResistantUnits")] CantTargetResistantUnits = 183,
    [pbr::OriginalName("MustTargetDazedUnits")] MustTargetDazedUnits = 184,
    [pbr::OriginalName("CantTargetDazedUnits")] CantTargetDazedUnits = 185,
    [pbr::OriginalName("CantLockdown")] CantLockdown = 186,
    [pbr::OriginalName("CantMindControl")] CantMindControl = 187,
    [pbr::OriginalName("MustTargetDestructibles")] MustTargetDestructibles = 188,
    [pbr::OriginalName("CantTargetDestructibles")] CantTargetDestructibles = 189,
    [pbr::OriginalName("MustTargetItems")] MustTargetItems = 190,
    [pbr::OriginalName("CantTargetItems")] CantTargetItems = 191,
    [pbr::OriginalName("NoCalldownAvailable")] NoCalldownAvailable = 192,
    [pbr::OriginalName("WaypointListFull")] WaypointListFull = 193,
    [pbr::OriginalName("MustTargetRace")] MustTargetRace = 194,
    [pbr::OriginalName("CantTargetRace")] CantTargetRace = 195,
    [pbr::OriginalName("MustTargetSimilarUnits")] MustTargetSimilarUnits = 196,
    [pbr::OriginalName("CantTargetSimilarUnits")] CantTargetSimilarUnits = 197,
    [pbr::OriginalName("CantFindEnoughTargets")] CantFindEnoughTargets = 198,
    [pbr::OriginalName("AlreadySpawningLarva")] AlreadySpawningLarva = 199,
    [pbr::OriginalName("CantTargetExhaustedResources")] CantTargetExhaustedResources = 200,
    [pbr::OriginalName("CantUseMinimap")] CantUseMinimap = 201,
    [pbr::OriginalName("CantUseInfoPanel")] CantUseInfoPanel = 202,
    [pbr::OriginalName("OrderQueueIsFull")] OrderQueueIsFull = 203,
    [pbr::OriginalName("CantHarvestThatResource")] CantHarvestThatResource = 204,
    [pbr::OriginalName("HarvestersNotRequired")] HarvestersNotRequired = 205,
    [pbr::OriginalName("AlreadyTargeted")] AlreadyTargeted = 206,
    [pbr::OriginalName("CantAttackWeaponsDisabled")] CantAttackWeaponsDisabled = 207,
    [pbr::OriginalName("CouldntReachTarget")] CouldntReachTarget = 208,
    [pbr::OriginalName("TargetIsOutOfRange")] TargetIsOutOfRange = 209,
    [pbr::OriginalName("TargetIsTooClose")] TargetIsTooClose = 210,
    [pbr::OriginalName("TargetIsOutOfArc")] TargetIsOutOfArc = 211,
    [pbr::OriginalName("CantFindTeleportLocation")] CantFindTeleportLocation = 212,
    [pbr::OriginalName("InvalidItemClass")] InvalidItemClass = 213,
    [pbr::OriginalName("CantFindCancelOrder")] CantFindCancelOrder = 214,
  }

  #endregion

}

#endregion Designer generated code
