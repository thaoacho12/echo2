import * as XLSX from 'xlsx';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ExcelService {

  exportToExcel(data: any[], fileName: string): void {
    // Chuyển đổi dữ liệu thành worksheet
    const worksheet = XLSX.utils.json_to_sheet(data);
    const workbook = { Sheets: { 'Data': worksheet }, SheetNames: ['Data'] };

    // Ghi workbook thành file Excel
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], { type: 'application/octet-stream' });

    // Tạo link tải file
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = `${fileName}.xlsx`; // Đặt tên file
    link.click();

    // Giải phóng URL sau khi tải
    window.URL.revokeObjectURL(link.href);
  }
}
