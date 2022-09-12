import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import {AudioService} from '../../../../services/audio.service';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {EditAudioComponent} from './edit-audio/edit-audio.component';
import {MergeImageDialogComponent} from '@app/components/dialogs/merge-image/merge-image.component';
import {Store} from '@ngrx/store';
import {CategoryDto} from '@app/models/dtos';
import {selectCategories} from '@app/modules/dashboard/assets/states/assets.reducers';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';

@Component({
  selector: 'app-audios',
  templateUrl: './audios.component.html',
  styleUrls: ['./audios.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class AudiosComponent implements OnInit {
  selectedFilter = '';
  categories: CategoryDto[];
  audios: Array<any>;
  constructor(private audioService: AudioService,
              private store: Store,
              public dialog: MatDialog,
              public snackBar: MatSnackBar) {
    const categories$ = this.store.select(selectCategories);
    categories$.subscribe(results => {
      this.categories = results;
    });
  }

  ngOnInit(): void {
    this.selectedFilter = 'most-viewed';
    this.loadAudios();
  }

  loadAudios(): void{
    this.audioService.getAudios().subscribe(results => {
      this.audios = results;
    });
  }

  toggle(audio): void {
    audio.checked = !audio.checked;
  }

  selectPlaylist(id): void {

  }

  openMergeImageDialog(): void {
    const dialogRef = this.dialog.open(MergeImageDialogComponent, {
      width: '500px',
      data: ''
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log('The dialog was closed', result);
    });
  }

  get selectedAudios(): any {
    if (!this.audios) {
      return [];
    }
    return this.audios.filter(v => v.checked).map(v => {
      v.assetType = 'Audio';
      return v;
    });
  }

  remove(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.title, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        this.audioService.deleteAudio(dto.id).subscribe(result => {
          // refresh the result
          this.loadAudios();
          this.snackBar.open('Audio with title \'' + dto.title + '\' has been successfully removed.', 'SUCCESS', {
            duration: 300000,
            panelClass: ['action-success']
          });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove audio with title \'' + dto.title + '\'', 'FAILURE', {
            duration: 120000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }

  edit(dto): void {
    const dialogRef = this.dialog.open(EditAudioComponent, {
      width: '700px',
      data: { dto }
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        for (let i = 0; i < this.audios.length; i++){
          if (this.audios[i].id === data.id) {
            this.audios[i] = data;
          }
        }
      }
    });
  }
}
