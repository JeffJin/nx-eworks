import {Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, ViewEncapsulation} from '@angular/core';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {ImageDto, VideoDto} from '@app/models/dtos';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-image-list',
  templateUrl: './image-list.component.html',
  styleUrls: ['./image-list.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class ImageListComponent implements OnInit, OnChanges {

  @Input() images: ImageDto[];
  @Input() selectedFilter: string;
  @Output() remove: EventEmitter<VideoDto> = new EventEmitter<VideoDto>();
  @Output() edit: EventEmitter<VideoDto> = new EventEmitter<VideoDto>();

  tableData: TableData;

  constructor(public dialog: MatDialog,
              public snackBar: MatSnackBar) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.tableData = {
      title: this.selectedFilter + ' Images',
      template: 'imageTemplate',
      headerRow: ['Title', 'Description', 'Created On'],
      dataRows: this.images
    };
  }


  ngOnInit(): void {
    this.tableData = {
      title: this.selectedFilter + ' Images',
      template: 'imageTemplate',
      headerRow: ['Title', 'Description', 'Created On'],
      dataRows: this.images
    };

  }

  removeItem(dto): void{
    this.remove.emit(dto);
  }

  editItem(dto): void {
    this.edit.emit(dto);
  }
}
