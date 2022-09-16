import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {PlaylistDto} from '../../../../models/dtos';
import {GroupService} from '../../../../services/group.service';
import {PlaylistService} from '../../../../services/playlist.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-create-playlist',
  templateUrl: './create-playlist.component.html',
  styleUrls: ['./create-playlist.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class CreatePlaylistComponent implements OnInit {
  today: Date = new Date();
  maxDate: Date = new Date(8640000000000000);
  groups = [];
  dto = new PlaylistDto({
    name: '',
    startDate: null,
    endDate: null,
    startTime: 28800,
    endTime: 79200,
    createdOn: new Date(),
    deviceGroups: [],
    subPlaylists: []
  });

  constructor(private groupService: GroupService, private playlistService: PlaylistService, private router: Router) { }

  ngOnInit(): void {
    this.groupService.loadGroups().subscribe(results => {
      this.groups = results.map(v => {
        v.checked = false;
        return v;
      });
    });
  }

  save(dto): void {
    this.playlistService.addPlaylist(dto).subscribe(results => {
      // redirect to list playlist
        this.router.navigateByUrl('dashboard/playlists/list');
      },
      error => {
      });
  }

  valid(dto: PlaylistDto): any {
    return dto.name && dto.startDate && dto.endDate;
  }

  toggle(item): void{
    item.checked = !item.checked;
  }

  cancel(): void {

  }

}
