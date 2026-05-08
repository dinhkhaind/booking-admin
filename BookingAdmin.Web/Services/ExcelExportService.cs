using BookingAdmin.Web.Models;
using ClosedXML.Excel;

namespace BookingAdmin.Web.Services;

public class ExcelExportService
{
    public byte[] GenerateDashboardExcel(
        string boatName,
        string period,
        List<string> periodLabels,
        List<EmployeeSalesRow> employeeSalesData,
        List<EmployeeSalesRow> employeeCancellationData,
        List<ChannelSummaryRow> channelSummaryData,
        List<EmployeeLastminRow> employeeLastminData,
        List<ChannelLastminRow> channelLastminData
    )
    {
        using (var workbook = new XLWorkbook())
        {
            // Sheet 1: Employee Sales
            CreateEmployeeSalesSheet(workbook, employeeSalesData, periodLabels);

            // Sheet 2: Employee Cancellations
            CreateEmployeeCancellationSheet(workbook, employeeCancellationData, periodLabels);

            // Sheet 3: Channel Summary
            CreateChannelSummarySheet(workbook, channelSummaryData);

            // Sheet 4: Employee Last-minute
            CreateEmployeeLastminSheet(workbook, employeeLastminData);

            // Sheet 5: Channel Last-minute
            CreateChannelLastminSheet(workbook, channelLastminData);

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }

    private void CreateEmployeeSalesSheet(XLWorkbook workbook, List<EmployeeSalesRow> data, List<string> periodLabels)
    {
        var ws = workbook.Worksheets.Add("Doanh số NV");

        ws.Cell(1, 1).Value = "Doanh số theo NV Sale";
        var titleCell = ws.Range(1, 1, 1, periodLabels.Count + 2);
        titleCell.Merge();
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header row
        ws.Cell(2, 1).Value = "Nhân viên";
        foreach (var (label, idx) in periodLabels.Select((l, i) => (l, i)))
        {
            ws.Cell(2, 2 + idx).Value = label;
        }
        ws.Cell(2, periodLabels.Count + 2).Value = "Tổng";

        var headerRow = ws.Range(2, 1, 2, periodLabels.Count + 2);
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Data rows
        int rowNum = 3;
        foreach (var row in data)
        {
            ws.Cell(rowNum, 1).Value = row.EmployeeName;
            foreach (var (label, idx) in periodLabels.Select((l, i) => (l, i)))
            {
                var value = row.PeriodSales.ContainsKey(label) ? row.PeriodSales[label] : 0;
                ws.Cell(rowNum, 2 + idx).Value = value;
                ws.Cell(rowNum, 2 + idx).Style.NumberFormat.Format = "#,##0";
            }
            ws.Cell(rowNum, periodLabels.Count + 2).Value = row.Total;
            ws.Cell(rowNum, periodLabels.Count + 2).Style.NumberFormat.Format = "#,##0";

            if (row.EmployeeName == "TỔNG CỘNG")
            {
                var totalRowRange = ws.Range(rowNum, 1, rowNum, periodLabels.Count + 2);
                totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);
                totalRowRange.Style.Font.Bold = true;
            }

            var rowRange = ws.Range(rowNum, 1, rowNum, periodLabels.Count + 2);
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            rowNum++;
        }

        ws.Columns().AdjustToContents();
    }

    private void CreateEmployeeCancellationSheet(XLWorkbook workbook, List<EmployeeSalesRow> data, List<string> periodLabels)
    {
        var ws = workbook.Worksheets.Add("Doanh số Huỷ NV");

        ws.Cell(1, 1).Value = "Doanh số Huỷ theo NV Sale";
        var titleCell = ws.Range(1, 1, 1, periodLabels.Count + 2);
        titleCell.Merge();
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(192, 0, 0);
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header row
        ws.Cell(2, 1).Value = "Nhân viên";
        foreach (var (label, idx) in periodLabels.Select((l, i) => (l, i)))
        {
            ws.Cell(2, 2 + idx).Value = label;
        }
        ws.Cell(2, periodLabels.Count + 2).Value = "Tổng";

        var headerRow = ws.Range(2, 1, 2, periodLabels.Count + 2);
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(192, 0, 0);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Data rows
        int rowNum = 3;
        foreach (var row in data)
        {
            ws.Cell(rowNum, 1).Value = row.EmployeeName;
            foreach (var (label, idx) in periodLabels.Select((l, i) => (l, i)))
            {
                var value = row.PeriodSales.ContainsKey(label) ? row.PeriodSales[label] : 0;
                ws.Cell(rowNum, 2 + idx).Value = value;
                ws.Cell(rowNum, 2 + idx).Style.NumberFormat.Format = "#,##0";
            }
            ws.Cell(rowNum, periodLabels.Count + 2).Value = row.Total;
            ws.Cell(rowNum, periodLabels.Count + 2).Style.NumberFormat.Format = "#,##0";

            if (row.EmployeeName == "TỔNG CỘNG")
            {
                var totalRowRange = ws.Range(rowNum, 1, rowNum, periodLabels.Count + 2);
                totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(242, 217, 217);
                totalRowRange.Style.Font.Bold = true;
            }

            var rowRange = ws.Range(rowNum, 1, rowNum, periodLabels.Count + 2);
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            rowNum++;
        }

        ws.Columns().AdjustToContents();
    }

    private void CreateChannelSummarySheet(XLWorkbook workbook, List<ChannelSummaryRow> data)
    {
        var ws = workbook.Worksheets.Add("Tổng kết Channel");

        ws.Cell(1, 1).Value = "Tổng kết theo Channel";
        var titleCell = ws.Range(1, 1, 1, 7);
        titleCell.Merge();
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 80);
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header row
        ws.Cell(2, 1).Value = "Channel";
        ws.Cell(2, 2).Value = "Lượng phòng";
        ws.Cell(2, 3).Value = "Doanh số";
        ws.Cell(2, 4).Value = "Lượng khách";
        ws.Cell(2, 5).Value = "Phòng huỷ";
        ws.Cell(2, 6).Value = "Doanh số huỷ";
        ws.Cell(2, 7).Value = "Khách huỷ";

        var headerRow = ws.Range(2, 1, 2, 7);
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 80);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data rows
        int rowNum = 3;
        foreach (var row in data)
        {
            ws.Cell(rowNum, 1).Value = row.ChannelName;
            ws.Cell(rowNum, 2).Value = row.TotalRooms;
            ws.Cell(rowNum, 3).Value = row.TotalSales;
            ws.Cell(rowNum, 4).Value = row.TotalCustomers;
            ws.Cell(rowNum, 5).Value = row.CancelledRooms;
            ws.Cell(rowNum, 6).Value = row.CancelledSales;
            ws.Cell(rowNum, 7).Value = row.CancelledCustomers;

            ws.Cell(rowNum, 3).Style.NumberFormat.Format = "#,##0";
            ws.Cell(rowNum, 6).Style.NumberFormat.Format = "#,##0";

            if (row.ChannelName == "TỔNG CỘNG")
            {
                var totalRowRange = ws.Range(rowNum, 1, rowNum, 7);
                totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 242, 225);
                totalRowRange.Style.Font.Bold = true;
            }

            var rowRange = ws.Range(rowNum, 1, rowNum, 7);
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            rowNum++;
        }

        ws.Columns().AdjustToContents();
    }

    private void CreateEmployeeLastminSheet(XLWorkbook workbook, List<EmployeeLastminRow> data)
    {
        var ws = workbook.Worksheets.Add("Lastmin NV");

        ws.Cell(1, 1).Value = "Lastmin theo NV Sale";
        var titleCell = ws.Range(1, 1, 1, 5);
        titleCell.Merge();
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 224);
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header row
        ws.Cell(2, 1).Value = "Nhân viên";
        ws.Cell(2, 2).Value = "Phòng 1 ngày";
        ws.Cell(2, 3).Value = "Doanh số 1 ngày";
        ws.Cell(2, 4).Value = "Phòng 3 ngày";
        ws.Cell(2, 5).Value = "Doanh số 3 ngày";

        var headerRow = ws.Range(2, 1, 2, 5);
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 224);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data rows
        int rowNum = 3;
        foreach (var row in data)
        {
            ws.Cell(rowNum, 1).Value = row.EmployeeName;
            ws.Cell(rowNum, 2).Value = row.Rooms1Day;
            ws.Cell(rowNum, 3).Value = row.Sales1Day;
            ws.Cell(rowNum, 4).Value = row.Rooms3Days;
            ws.Cell(rowNum, 5).Value = row.Sales3Days;

            ws.Cell(rowNum, 3).Style.NumberFormat.Format = "#,##0";
            ws.Cell(rowNum, 5).Style.NumberFormat.Format = "#,##0";

            if (row.EmployeeName == "TỔNG CỘNG")
            {
                var totalRowRange = ws.Range(rowNum, 1, rowNum, 5);
                totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 236, 242);
                totalRowRange.Style.Font.Bold = true;
            }

            var rowRange = ws.Range(rowNum, 1, rowNum, 5);
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            rowNum++;
        }

        ws.Columns().AdjustToContents();
    }

    private void CreateChannelLastminSheet(XLWorkbook workbook, List<ChannelLastminRow> data)
    {
        var ws = workbook.Worksheets.Add("Lastmin Channel");

        ws.Cell(1, 1).Value = "Lastmin theo Channel";
        var titleCell = ws.Range(1, 1, 1, 5);
        titleCell.Merge();
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 224);
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontColor = XLColor.White;
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Header row
        ws.Cell(2, 1).Value = "Channel";
        ws.Cell(2, 2).Value = "Phòng 1 ngày";
        ws.Cell(2, 3).Value = "Doanh số 1 ngày";
        ws.Cell(2, 4).Value = "Phòng 3 ngày";
        ws.Cell(2, 5).Value = "Doanh số 3 ngày";

        var headerRow = ws.Range(2, 1, 2, 5);
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 176, 224);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Font.FontColor = XLColor.White;
        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Data rows
        int rowNum = 3;
        foreach (var row in data)
        {
            ws.Cell(rowNum, 1).Value = row.ChannelName;
            ws.Cell(rowNum, 2).Value = row.Rooms1Day;
            ws.Cell(rowNum, 3).Value = row.Sales1Day;
            ws.Cell(rowNum, 4).Value = row.Rooms3Days;
            ws.Cell(rowNum, 5).Value = row.Sales3Days;

            ws.Cell(rowNum, 3).Style.NumberFormat.Format = "#,##0";
            ws.Cell(rowNum, 5).Style.NumberFormat.Format = "#,##0";

            if (row.ChannelName == "TỔNG CỘNG")
            {
                var totalRowRange = ws.Range(rowNum, 1, rowNum, 5);
                totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 236, 242);
                totalRowRange.Style.Font.Bold = true;
            }

            var rowRange = ws.Range(rowNum, 1, rowNum, 5);
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            rowNum++;
        }

        ws.Columns().AdjustToContents();
    }
}
