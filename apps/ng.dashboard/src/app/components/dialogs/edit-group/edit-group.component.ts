import {Component, Inject, OnInit, ViewEncapsulation} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';
import {GroupService} from '@app/services/group.service';
import {GroupDto} from '@app/models/dtos';

@Component({
  selector: 'app-edit-group-dialog',
  templateUrl: './edit-group.component.html',
  styleUrls: ['./edit-group.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class EditGroupDialogComponent implements OnInit {

  constructor(private groupService: GroupService,
              public dialogRef: MatDialogRef<EditGroupDialogComponent>,
              @Inject(MAT_DIALOG_DATA) public group: GroupDto) {
  }

  ngOnInit(): void {
    console.log('edit group', this.group);
  }

  cancel(): void {
    this.dialogRef.close();
  }

  save(dto: GroupDto): void {
    console.log('update group', this.group);

    this.groupService.updateGroup(dto.id, dto).subscribe((result) => {
      this.dialogRef.close(result);
    });
  }


}
