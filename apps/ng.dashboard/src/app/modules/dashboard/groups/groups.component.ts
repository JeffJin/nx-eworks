import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {GroupService} from '../../../services/group.service';
import {GroupDto, PlaylistDto} from '../../../models/dtos';
import {ConfirmDeleteDialogComponent} from '@app/components/dialogs/confirm-delete/confirm-delete';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {AddGroupDialogComponent} from '@app/components/dialogs/create-group/add-group.component';
import {EditGroupDialogComponent} from '@app/components/dialogs/edit-group/edit-group.component';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-groups',
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class GroupsComponent implements OnInit {
  groups: any[];
  keywords = '';
  selectedFilter = 'name';
  tableData: TableData;

  constructor(private groupService: GroupService, public dialog: MatDialog, public snackBar: MatSnackBar) {
  }

  ngOnInit(): void {
    this.loadGroups();
  }

  search(key): any {
    this.groupService.searchGroups(key).subscribe(results => {
      console.log('search results', results);
      // refresh the result
      this.groups = results.map(v => {
        v.checked = false;
        return v;
      });
    });
  }

  addNew(): void {
    const dto = new GroupDto(
      {
        name: '',
        createdOn: new Date()
      }
    );
    const dialogRef = this.dialog.open(AddGroupDialogComponent, {
      data: dto
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data) {
        this.groupService.addGroup(data).subscribe(results => {
            this.loadGroups();
          },
          error => {
            this.snackBar.open('Failed to register new device ', 'FAILURE', {
              duration: 5000,
              panelClass: ['action-error']
            });
          });
      }
    });
  }

  loadGroups(): void {
    this.groupService.loadGroups().subscribe(results => {
      console.log('load group results', results);
      // refresh the result
      this.groups = results.map(v => {
        v.checked = false;
        return v;
      });
      this.tableData = {
        title: 'All Groups',
        template: 'groupTemplate',
        headerRow: ['Name', 'Devices', 'Playlists', 'Created On'],
        dataRows: this.groups
      };
    });
  }

  toggle(item): void {
    item.checked = !item.checked;
  }

  edit(id: string): void {
    this.groupService.getGroup(id).subscribe(dto => {

      const dialogRef = this.dialog.open(EditGroupDialogComponent, {
        data: dto
      });

      dialogRef.afterClosed().subscribe(data => {
        if (data && data.id) {
          for (let i = 0; i < this.groups.length; i++) {
            if (this.groups[i].id === data.id) {
              this.groups[i] = data;
            }
          }
        }
      });
    });
  }

  remove(dto): void {
    const dialogRef = this.dialog.open(ConfirmDeleteDialogComponent, {
      width: '360px',
      data: {title: dto.name, result: ''}
    });

    dialogRef.afterClosed().subscribe(data => {
      if (data && data.result === 'continue') {
        this.groupService.deleteGroup(dto.id).subscribe(result => {
          // refresh the result
          this.loadGroups();
          this.snackBar.open('Group with name \'' + dto.name + '\' has been successfully removed.', 'SUCCESS', {
            duration: 3000,
            panelClass: ['action-success']
          });
        }, (err) => {
          console.error(err);
          this.snackBar.open('Failed to remove group with name \'' + dto.name + '\'', 'FAILURE', {
            duration: 5000,
            panelClass: ['action-error']
          });
        });
      }
    });
  }

  mergeGroups(selectedGroups: Array<any>): void {

  }

  get selectedGroups(): GroupDto[] {
    if (!this.groups) {
      return [];
    }
    return this.groups.filter(v => v.checked);
  }


}
