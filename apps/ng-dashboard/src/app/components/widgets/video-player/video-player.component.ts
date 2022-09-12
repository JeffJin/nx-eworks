import {Component, ElementRef, Input, OnDestroy, OnInit, ViewChild, ViewEncapsulation} from '@angular/core';
import {VideoDto} from '../../../models/dtos';
import videojs from 'video.js';
import {ActivatedRoute} from '@angular/router';

@Component({
  selector: 'app-video-player',
  templateUrl: './video-player.component.html',
  styleUrls: ['./video-player.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class VideoPlayerComponent implements OnInit, OnDestroy {
  @ViewChild('target', {static: true}) target: ElementRef;
  @Input() options: {
    fluid?: boolean,
    aspectRatio?: string,
    autoplay?: boolean,
    controls?: boolean,
    sources: {
      src: string,
      type: string,
    }[],
    title?: string,
    desc?: string,
    createdOn?: Date,
  } = {
    fluid: true,
    aspectRatio: '16:9',
    autoplay: true,
    controls: true,
    sources: [{
      src: '',
      type: 'video/mp4',
    }],
    title: '',
    desc: '',
    createdOn: new Date(),
  };

  player: videojs.Player;

  constructor(private elementRef: ElementRef) {
    this.options.aspectRatio =  this.options.aspectRatio ?? '16:9';
    this.options.autoplay =  this.options.autoplay ?? true;
    this.options.controls =  this.options.controls ?? true;
    this.options.sources = this.options.sources.map(s => {
      s.type = s.type ?? 'video/mp4';
      return s;
    });
  }

  ngOnInit(): void {
    console.log('this.options', this.options);
    this.player = videojs(this.target.nativeElement, this.options, function onPlayerReady(): void {
      console.log('onPlayerReady', this);
    });
  }

  ngOnDestroy(): void {
    // destroy player
    if (this.player) {
      this.player.dispose();
    }
  }

}
