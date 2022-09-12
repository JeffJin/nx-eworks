import {Component, ElementRef, Inject, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';

import {Utils} from '../../../../services/utils';
import {PlaylistService} from '../../../../services/playlist.service';
import {Asset, AudioDto, ImageDto, PlaylistDto, PlaylistItemDto, VideoDto} from '../../../../models/dtos';
import * as moment from 'moment';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {MatStepper} from '@angular/material/stepper';

@Component({
  selector: 'app-add-to-playlist',
  templateUrl: './add-to-playlist.component.html',
  styleUrls: ['./add-to-playlist.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class AddToPlaylistComponent implements OnInit {
  @ViewChild('stepper') stepper: MatStepper;

  playlistItems: Array<PlaylistItemDto>;
  playlists: Array<PlaylistDto>;
  selectedPlaylist: PlaylistDto;
  today = new Date();

  isPlaying = false;
  selectedSubPlaylistIndex = 0;
  selectedTemplate: any = {index: 1};

  playlistStartTime = new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate(), 9);
  playlistEndTime = new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate(), 21);

  get startTimeString(): string {
    return moment(this.playlistStartTime).format('HH:mm a');
  }

  get endTimeString(): string {
    return moment(this.playlistEndTime).format('HH:mm a');
  }

  get duration(): string {
    const time = this.playlistItems.reduce((a, b) => {
      return a + b.duration;
    }, 0);
    return moment.utc(time * 1000).format('HH:mm:ss');
  }

  constructor(private utils: Utils, private playlistService: PlaylistService,
              public dialogRef: MatDialogRef<AddToPlaylistComponent>, public snackBar: MatSnackBar,
              @Inject(MAT_DIALOG_DATA) public data: any) {

  }

  ngOnInit(): void {
    this.loadPlaylists();
    this.initPlaylistItems(this.data);

    // this.sortableOptions = {
    //   onUpdate: (evt: any) => {
    //     this.swapPlaylistItems(evt.oldIndex, evt.newIndex, this.selectedSubPlaylistIndex);
    //   }
    // };
  }

  loadPlaylists(): void {
    this.playlistService.loadPlaylists().subscribe(results => {
      this.playlists = results;
    });
  }

  initPlaylistItems(assets): void {
    this.playlistItems = [];

    for (let i = 0; i < assets.length; i++) {
      const asset = assets[i];
      const item = this.createPlaylistItem(asset, i);

      this.playlistItems.push(item);
    }

  }

  createPlaylistItem(asset: Asset, index: number): PlaylistItemDto {
    if (asset instanceof VideoDto) {
      return new PlaylistItemDto(
        {
          index,
          subPlaylistId: '',
          mediaAssetId: asset.id,
          duration: Math.round(asset.duration),
          media: asset,
          cacheLocation: ''
        }
      );
    } else if (asset instanceof AudioDto) {
      return new PlaylistItemDto(
        {
          index,
          subPlaylistId: '',
          mediaAssetId: asset.id,
          duration: Math.round(asset.duration),
          media: asset,
          cacheLocation: ''
        }
      );
    } else if (asset instanceof ImageDto) {
      return new PlaylistItemDto(
        {
          index,
          subPlaylistId: '',
          mediaAssetId: asset.id,
          media: asset,
          cacheLocation: ''
        }
      );
    } else {
      return null;
    }
  }

  convertTimeSeconds(seconds: number): any {
    const date = new Date(null);
    date.setSeconds(seconds); // specify value for SECONDS here
    return date.toISOString().substr(11, 8);
  }

  convertDuration(secs: number): string {
    const hours = Math.floor(secs / (60 * 60));

    const divisor_for_minutes = secs % (60 * 60);
    const minutes = Math.floor(divisor_for_minutes / 60);

    const divisor_for_seconds = divisor_for_minutes % 60;
    const seconds = Math.ceil(divisor_for_seconds);

    return hours + 'h ' + minutes + 'm ' + seconds + 's';
  }

  convertTimeString(timeStr: string) {
    const temp = timeStr.split(':');
    return (+temp[0]) * 60 * 60 + (+temp[1]) * 60 + (+temp[2]);
  }

  openStartTimePicker(item) {
    // const amazingTimePicker = this.atp.open({
    //   time: moment(this.playlistStartTime).format('HH:mm')
    // });
    // amazingTimePicker.afterClose().subscribe(timeString => {
    //   item.startTime = this.convertTimeString(timeString);
    // });

  }

  openEndTimePicker(item) {
    // const amazingTimePicker = this.atp.open({
    //   time: moment(this.playlistEndTime).format('HH:mm')
    // });
    // amazingTimePicker.afterClose().subscribe(timeString => {
    //   item.endTime = this.convertTimeString(timeString);
    // });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  swapPlaylistItems(fromIndex: number, toIndex: number, subIndex: number) {
    const fromItem = this.selectedPlaylist.subPlaylists[subIndex].playlistItems.find(pi => pi.index === fromIndex);
    const toItem = this.selectedPlaylist.subPlaylists[subIndex].playlistItems.find(pi => pi.index === toIndex);
    console.log('swap items', fromItem, toItem);
    fromItem.index = toIndex;
    toItem.index = fromIndex;

    console.log('after swapping', this.selectedPlaylist);
  }

  private createPlaylistCopy(pl: PlaylistDto): any {
    const copy: PlaylistDto = JSON.parse(JSON.stringify(pl));
    copy.startDate = pl.startDate;
    copy.endDate = pl.endDate;
    copy.updatedOn = pl.updatedOn;
    copy.subPlaylists.forEach(spl => {
      spl.playlistItems.forEach(pi => delete pi.media);
    });
    return copy;
  }

  save(): void {
    const copy = this.createPlaylistCopy(this.selectedPlaylist);
    console.log('save playlist', copy);
    this.playlistService.updatePlaylist(copy).subscribe(result => {
      this.stepper.next();
    }, error => {
      this.snackBar.open('Failed to update playlist \'' + copy.name + '\'', 'FAILURE', {
        duration: 5000,
        panelClass: ['action-error']
      });
    });
  }

  publish(): void {
    this.playlistService.publishPlaylist(this.selectedPlaylist.id).subscribe(result => {
      this.dialogRef.close(result);
    }, error => {
      this.snackBar.open('Failed to publish playlist \'' + this.selectedPlaylist.name + '\'', 'FAILURE', {
        duration: 5000,
        panelClass: ['action-error']
      });
    });
  }

  playlistSelected(pl: PlaylistDto) {
    if (!pl) {
      return;
    }
    this.selectedPlaylist = pl;
    const subPlaylist = this.selectedPlaylist.subPlaylists[this.selectedSubPlaylistIndex];
    subPlaylist.playlistItems = this.playlistItems.map(pi => {
      pi.subPlaylistId = subPlaylist.id;
      return pi;
    });
  }

  playPreview(): void {
    this.isPlaying = true;
  }

  videoPlayed(evt): void {

  }

  selectTemplate(templateIndex: number): void {
    this.selectedTemplate.index = templateIndex;
  }
}
