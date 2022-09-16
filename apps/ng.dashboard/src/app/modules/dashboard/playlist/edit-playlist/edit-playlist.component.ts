import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {PlaylistDto} from '../../../../models/dtos';
import {PlaylistService} from '../../../../services/playlist.service';
import {GroupService} from '../../../../services/group.service';
import { combineLatest } from 'rxjs';
import {ActivatedRoute, Router} from '@angular/router';
import {VideoService} from '../../../../services/video.service';

@Component({
  selector: 'app-edit-playlist',
  templateUrl: './edit-playlist.component.html',
  styleUrls: ['./edit-playlist.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditPlaylistComponent implements OnInit {
  today: Date = new Date();
  groups = [];
  assets = [];
  playlist: PlaylistDto;
  playlistId: string;
  firstItemStartTime: string;

  constructor(private playlistService: PlaylistService, private groupService: GroupService, private videoService: VideoService,
              private router: Router, private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.playlistId = params.id;
    });
  }

  ngOnInit(): void {
    combineLatest(this.playlistService.getPlaylist(this.playlistId, true), this.groupService.loadGroups(),
      this.videoService.getVideos())
      .subscribe(([playlist, groups, assets]) => {
          this.playlist = playlist;

          this.groups = groups.map(v => {
            if (this.playlist.deviceGroups) {
              v.checked = !!this.playlist.deviceGroups.find(g => g.id === v.id);
            }
            else {
              v.checked = false;
            }

            return v;
          });

          this.assets = assets.map(v => {
            v.selected = false;
            v.assetType = 'Video';
            return v;
          });

        },
        (err) => {
          console.error(err);
        });
  }

  cancel(): void {
  }


  convertTimeSeconds(seconds: number){
    const date = new Date(null);
    date.setSeconds(seconds); // specify value for SECONDS here
    return date.toISOString().substr(11, 8);
  }

  convertDuration(secs: number){
    const hours = Math.floor(secs / (60 * 60));

    const divisor_for_minutes = secs % (60 * 60);
    const minutes = Math.floor(divisor_for_minutes / 60);

    const divisor_for_seconds = divisor_for_minutes % 60;
    const seconds = Math.ceil(divisor_for_seconds);

    return hours + 'h ' + minutes + 'm ' + seconds + 's';
  }

  convertTimeString(timeStr: string){
    const temp = timeStr.split(':');
    return (+temp[0]) * 60 * 60 + (+temp[1]) * 60 + (+temp[2]);
  }

  save(dto: PlaylistDto) {

    const selectedGroups = this.groups.filter(g => g.checked);
    dto.deviceGroups = selectedGroups;

    this.playlistService.updatePlaylist(dto).subscribe((result) => {
      this.router.navigateByUrl('dashboard/playlist/list');
    });
  }

  valid(dto: PlaylistDto){
    return dto && dto.name && dto.startDate && dto.endDate;
  }
}
