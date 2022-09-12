import {Component, ComponentRef, ElementRef, Inject, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {DomSanitizer} from '@angular/platform-browser';
import {VideoDto} from '../../models/dtos';

import {WindowRef} from '../../services/window-ref';
import {DOCUMENT} from '@angular/common';
import {HubConnection, HubConnectionBuilder, LogLevel} from '@aspnet/signalr';
import {Utils} from '../../services/utils';
import {VideoService} from '../../services/video.service';
import {environment} from '../../../environments/environment';
import {CacheService} from '../../services/cache.service';

@Component({
  selector: 'app-full-screen-video',
  templateUrl: './full-screen-video.component.html',
  styleUrls: ['./full-screen-video.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class FullScreenVideoComponent implements OnInit {
  @ViewChild('playerElm') playerElm: ElementRef;

  private _hubConnection: HubConnection;

  currentVideo: VideoDto = null;
  currentIndex = -1;
  videos: VideoDto[] = [];
  player: any;
  isPlaying = false;

  constructor(private sanitizer: DomSanitizer,
              private utils: Utils,
              private videoService: VideoService,
              private windowRef: WindowRef,
              private cacheService: CacheService,
              @Inject(DOCUMENT) private document: any) {
  }

  ngOnInit(): void {
    this.videoService.getVideos().subscribe(results => {
      this.videos = results.map(r => {
        return {
          id: r.id,
          sourceId: r.sourceId,
          url: r.progressiveUrl,
          category: r.category,
          type: r.sourceType,
          createdBy: r.email,
          title: r.title,
          description: r.description
        };
      });
    });
    // init player
    this.initPlayer();
  }

  initPlayer(): void {
    this.player = {on: () => {}};
    this.player.on('stateChange', (event) => {
      console.log('stateChange', event.data);
      if (event.data === 0) {
        this.playNext();
      }

      if (event.data === -1) {
        const iframe = this.document.getElementById('player');

        const requestFullScreen = iframe.requestFullScreen || iframe.mozRequestFullScreen || iframe.webkitRequestFullScreen;
        if (requestFullScreen) {
          requestFullScreen.bind(iframe)();
        }
      }
    });
  }

  playNext(): void {
    this.isPlaying = true;
    this.currentIndex++;
    if (this.currentIndex >= this.videos.length) {
      this.currentIndex = 0;
    }
    this.currentVideo = this.videos[this.currentIndex];

    // 'loadVideoById' is queued until the player is ready to receive API calls.
    this.player.loadVideoById(this.currentVideo.id);

    this.player.playVideo();
  }


  getUrl(url: string): any {
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }


  startHubConnection(): void {
    // const transportType = TransportType[this.utils.getParameterByName('transport')] || TransportType.WebSockets;

    const jwtToken = this.cacheService.getToken();
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}eventing?t=${jwtToken}`)
      .configureLogging(LogLevel.Critical)
      .build();

    this._hubConnection.on('Send', (data: any) => {
      const received = `Received: ${(data)}`;
      console.log(received);
    });

    this._hubConnection.start()
      .then(() => {
        console.log('Hub connection started');
      })
      .catch(err => {
        console.log('Error while establishing connection');
      });
  }

  stopHubConnection(): void {
    this._hubConnection.stop();
    console.log('Hub connection stopped');
  }


}
