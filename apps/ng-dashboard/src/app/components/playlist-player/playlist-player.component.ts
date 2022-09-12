import {Component, OnInit, ViewEncapsulation, Input, ElementRef, ViewChild, Output, EventEmitter} from '@angular/core';
import {PlaylistItemDto} from '../../models/dtos';
import {DomSanitizer} from '@angular/platform-browser';

@Component({
  selector: 'app-playlist-player',
  templateUrl: './playlist-player.component.html',
  styleUrls: ['./playlist-player.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class PlaylistPlayerComponent implements OnInit {
  @ViewChild('player') player: ElementRef;

  @Input() playlistItems: PlaylistItemDto[];
  @Output() onFinished = new EventEmitter<number>();
  currentAssetIndex = -1;
  sanitizedvideoUrl = null;

  constructor(private sanitizer: DomSanitizer) { }

  ngOnInit() {
    this.player.nativeElement.addEventListener('ended', () => {
      console.log('Current video finished playing, playing next');
      this.onFinished.emit(this.currentAssetIndex);
      this.playNext();
    });
    this.playNext();
  }

  playNext(){
    this.currentAssetIndex++;
    if(this.currentAssetIndex >= this.playlistItems.length){
      this.currentAssetIndex = 0;
    }
    if(this.currentAssetIndex >= 0 && this.currentAssetIndex < this.playlistItems.length){
      const item = this.playlistItems[this.currentAssetIndex];
      // this.sanitizedvideoUrl = this.sanitizer.bypassSecurityTrustResourceUrl();
      setTimeout(() => {
        this.player.nativeElement.src = item.media.cloudUrl;
        return this.player.nativeElement.play();
      }, 500);
    }
  }

}
