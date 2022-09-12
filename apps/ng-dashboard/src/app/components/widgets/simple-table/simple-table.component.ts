import {ChangeDetectionStrategy, Component, Input, OnInit, TemplateRef, ViewEncapsulation} from '@angular/core';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-simple-table',
  templateUrl: './simple-table.component.html',
  styleUrls: ['./simple-table.component.scss'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SimpleTableComponent implements OnInit {

  @Input() tableData: TableData;
  @Input() icon = 'assignment';
  @Input() plainTable?: boolean;
  @Input() altRowColors?: boolean;
  @Input() altRowMonoColor?: boolean;
  @Input() tableRowTemplate?: TemplateRef<any>;

  constructor() { }

  ngOnInit(): void {
  }

  changeTitle(textContent: string): void {
    this.tableData.title = textContent + ' Videos';
  }
}
