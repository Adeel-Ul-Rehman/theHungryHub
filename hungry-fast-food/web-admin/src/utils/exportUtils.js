import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import 'jspdf-autotable';

// Export to CSV
export const exportToCSV = (data, fileName) => {
  if (!data || !data.length) return;
  const worksheet = XLSX.utils.json_to_sheet(data);
  const csvOutput = XLSX.utils.sheet_to_csv(worksheet);
  const blob = new Blob([csvOutput], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  const url = URL.createObjectURL(blob);
  link.setAttribute('href', url);
  link.setAttribute('download', `${fileName}_${new Date().toISOString().slice(0, 10)}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

// Export to Excel (.xlsx)
export const exportToExcel = (data, fileName, sheetName = 'Report') => {
  if (!data || !data.length) return;
  const worksheet = XLSX.utils.json_to_sheet(data);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, sheetName);
  XLSX.writeFile(workbook, `${fileName}_${new Date().toISOString().slice(0, 10)}.xlsx`);
};

// Export to PDF
export const exportToPDF = (title, headers, rows, fileName) => {
  const doc = new jsPDF();
  
  // Header Title
  doc.setFontSize(18);
  doc.setTextColor(249, 115, 22); // Orange primary
  doc.text("HUNGRY HUB — " + title.toUpperCase(), 14, 20);

  doc.setFontSize(10);
  doc.setTextColor(100, 116, 139);
  doc.text(`Generated on: ${new Date().toLocaleString()}`, 14, 28);
  doc.text("Official Restaurant Operations Report", 14, 34);

  // Table
  doc.autoTable({
    startY: 40,
    head: [headers],
    body: rows,
    theme: 'grid',
    headStyles: { fillColor: [249, 115, 22], textColor: [255, 255, 255], fontStyle: 'bold' },
    alternateRowStyles: { fillColor: [248, 250, 252] },
    margin: { top: 40 }
  });

  doc.save(`${fileName}_${new Date().toISOString().slice(0, 10)}.pdf`);
};
