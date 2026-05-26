using System;
using System.Collections.Generic;

namespace dienthoai.Models;

public partial class TuCamDanhGium
{
    public int MaTu { get; set; }

    public string TuKhoa { get; set; } = null!;

    public string? GhiChu { get; set; }
}
