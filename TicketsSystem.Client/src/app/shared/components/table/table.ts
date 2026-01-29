import { Component, ContentChild, EventEmitter, Input, Output, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';

export interface TableColumn {
  key: string;
  header: string;
  width?: string;
}

export interface TablePagination {
  currentPage: number;
  pageSize: number;
  totalItems: number;
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  templateUrl: './table.html'
})
export class Table<T = any> {
  @Input() title: string = '';
  @Input() columns: TableColumn[] = [];
  @Input() data: T[] = [];
  @Input() pagination?: TablePagination;
  @Input() showFilter: boolean = true;
  @Input() showExport: boolean = true;

  @ContentChild('cellTemplate') cellTemplate!: TemplateRef<any>;

  @Output() filter = new EventEmitter<void>();
  @Output() exportData = new EventEmitter<void>();
  @Output() pageChange = new EventEmitter<number>();
  @Output() rowClick = new EventEmitter<T>();

  get totalPages(): number {
    if (!this.pagination) return 0;
    return Math.ceil(this.pagination.totalItems / this.pagination.pageSize);
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const total = this.totalPages;
    const current = this.pagination?.currentPage ?? 1;

    let start = Math.max(1, current - 1);
    let end = Math.min(total, start + 2);

    if (end - start < 2) {
      start = Math.max(1, end - 2);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  get showingFrom(): number {
    if (!this.pagination) return 0;
    return (this.pagination.currentPage - 1) * this.pagination.pageSize + 1;
  }

  get showingTo(): number {
    if (!this.pagination) return 0;
    return Math.min(
      this.pagination.currentPage * this.pagination.pageSize,
      this.pagination.totalItems
    );
  }

  onFilter(): void {
    this.filter.emit();
  }

  onExport(): void {
    this.exportData.emit();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  onRowClick(row: T): void {
    this.rowClick.emit(row);
  }

  getCellValue(row: T, key: string): any {
    return (row as any)[key];
  }
}
