using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace Wyam.Tables
{
    internal static class ExcelFile
    {
        public static IEnumerable<IEnumerable<string>> GetAllRecords(Stream stream, int sheetNumber = 0)
        {
            using (var excel = new ExcelPackage(stream))
            {
                excel.Compatibility.IsWorksheets1Based = false;
                if (sheetNumber > excel.Workbook.Worksheets.Count)
                {
                    return null;
                }

                var sheet = excel.Workbook.Worksheets[sheetNumber];

                return GetAllRecords(sheet);
            }
        }

        public static IEnumerable<IEnumerable<string>> GetAllRecords(ExcelWorksheet sheet)
        {
            var dimension = sheet.Dimension;

            if (dimension == null)
            {
                return null;
            }

            var rowList = new List<List<string>>();
            int rowCount = dimension.Rows;
            int columnCount = dimension.Columns;

            for (var r = 1; r <= rowCount; r++)
            {
                var rowValues = new List<string>(columnCount);
                for (var c = 1; c <= columnCount; c++)
                {
                    var cell = sheet.Cells[r, c].FirstOrDefault();
                    rowValues.Add(cell?.Value?.ToString());
                }

                rowList.Add(rowValues);
            }

            return rowList;
        }
    }
}
