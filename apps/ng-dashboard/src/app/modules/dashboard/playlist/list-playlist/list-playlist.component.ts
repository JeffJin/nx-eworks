import { Component, OnInit } from '@angular/core';
import {PlaylistService} from '../../../../services/playlist.service';
import {VideoService} from '../../../../services/video.service';
import {PlaylistDto} from '../../../../models/dtos';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatDialog} from '@angular/material/dialog';
import {TableData} from '@app/models/table-data';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';

@Component({
  selector: 'app-list-playlist',
  templateUrl: './list-playlist.component.html',
  styleUrls: ['./list-playlist.component.scss']
})
export class ListPlaylistComponent implements OnInit {
  playlists: Array<PlaylistDto> = [];
  assets: Array<any> = [];
  keywords = '';
  tableData: TableData;

  constructor(private videoService: VideoService, private playlistService: PlaylistService,
              public dialog: MatDialog, public snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.loadPlaylists();
  }

  search(key): void{
    this.playlistService.searchPlaylist(key).subscribe(results => {
      this.playlists = results.map(v => {
        v.checked = false;
        return v;
      });
    });
  }

  loadPlaylists(): void{
    this.playlistService.loadPlaylists().subscribe(results => {
      // refresh the results
      this.playlists = results.map(v => {
        v.checked = false;
        return v;
      });


      this.tableData = {
        title: 'Playlists',
        template: 'playlistTemplate',
        headerRow: ['Name', 'Sub playlists', 'Groups', 'Created On', 'Start Date', 'End Date'],
        dataRows: this.playlists
      };
    });
  }

  toggle(item): void{
    item.checked = !item.checked;
  }

  addNew(): void {

  }

  publish(dto: any): void{
    this.playlistService.publishPlaylist(dto.id).subscribe(result => {
      if(result) {
        this.snackBar.open('Playlist with name \'' + dto.name + '\' has been successfully published.', 'SUCCESS', {
          duration: 3000,
          panelClass: ['action-success']
        });
      }
      else {
        this.snackBar.open('Failed to publish playlist with name \'' + dto.name + '\'', 'FAILURE', {
          duration: 5000,
          panelClass: ['action-error']
        });
      }
    }, (err) => {
      this.snackBar.open('Failed to publish playlist with name \'' + dto.name + '\'', 'FAILURE', {
        duration: 5000,
        panelClass: ['action-error']
      });
    });
  }


  remove(dto){
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.name, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if(data && data.result === 'continue'){
        this.playlistService.deletePlaylist(dto.id).subscribe(result => {
          // refresh the result
          this.loadPlaylists();
          this.snackBar.open('Playlist with name \'' + dto.name + '\' has been successfully removed.', 'SUCCESS', {
            duration: 3000,
            panelClass: ['action-success']
          });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove playlist with name \'' + dto.name + '\'', 'FAILURE', {
            duration: 5000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }

  edit(playlist: any): void {

  }
}
