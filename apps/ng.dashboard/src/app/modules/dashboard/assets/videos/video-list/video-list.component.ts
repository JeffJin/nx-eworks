import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output, SimpleChanges,
  ViewEncapsulation
} from '@angular/core';
import {AddToPlaylistComponent} from '../../add-to-playlist/add-to-playlist.component';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {VideoDto} from '@app/models/dtos';
import {AddWeatherForecastDialogComponent} from '@app/components/dialogs/add-weather-forecast/add-weather-forecast';
import {TableData} from '@app/models/table-data';
import {EditVideoComponent} from '@app/modules/dashboard/assets/videos/edit-video/edit-video.component';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {AddTextDialogComponent} from '@app/components/dialogs/add-text/add-text';
import {AddPipDialogComponent} from '@app/components/dialogs/add-pip/add-pip';

@Component({
  selector: 'app-video-list',
  templateUrl: './video-list.component.html',
  styleUrls: ['./video-list.component.scss'],
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VideoListComponent implements OnChanges {
  @Input() videos: VideoDto[];
  @Input() selectedFilter: string;

  tableData: TableData;

  constructor(
    public dialog: MatDialog,
    public snackBar: MatSnackBar,
  ) {
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.tableData = {
      title: this.selectedFilter + ' Videos',
      template: 'videoTemplate',
      headerRow: ['Title', 'Description', 'Created On'],
      dataRows: this.videos
    };
  }

  openAddTextDialog(): void {
    const dialogRef = this.dialog.open(AddTextDialogComponent, {
      width: '500px',
      data: '',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed', result);
    });
  }

  openAddPipDialog(): void {
    const dialogRef = this.dialog.open(AddPipDialogComponent, {
      width: '500px',
      data: '',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed', result);
    });
  }

  openAddWeatherForecastDialog(): void {
    const dialogRef = this.dialog.open(AddWeatherForecastDialogComponent, {
      width: '500px',
      data: '',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed', result);
    });
  }

  removeItem(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.title, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        // TODO dispatch a delete action
      }
    });
  }

  process(dto): void {
    const dialogRef = this.dialog.open(EditVideoComponent, {
      width: '700px',
      data: {dto}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        // TODO publish the change
      }
    });
  }

  addToPlaylist(assets): void {

    const dialogRef = this.dialog.open(AddToPlaylistComponent, {
      width: '90%',
      data: assets,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(data => {
    });
  }
}
