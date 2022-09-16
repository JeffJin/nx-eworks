import {Component, EventEmitter, Input, OnInit, Output, TemplateRef, ViewEncapsulation} from '@angular/core';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-action-table',
  templateUrl: './action-table.component.html',
  styleUrls: ['./action-table.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class ActionTableComponent implements OnInit {

  @Input() tableData: TableData;
  @Input() title: string;
  @Input() icon = 'assignment';
  @Input() plainTable?: boolean;
  @Input() altRowColors?: boolean;
  @Input() altRowMonoColor?: boolean;
  @Input() tableRowTemplate?: TemplateRef<any>;
  keywords: string;
  pageNum: number = 10;

  @Output() search: EventEmitter<string> = new EventEmitter<string>();
  @Output() pagination: EventEmitter<number> = new EventEmitter<number>();

  constructor() { }

  ngOnInit(): void {
  }

  searchKeyword(keywords: string): void {
    this.search.emit(keywords);
  }

  paginationChanged(pageNum: number): void {
    this.pagination.emit(pageNum);
  }
}
