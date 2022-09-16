import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import {GroupService} from '@app/services/group.service';
import {MatDialog} from '@angular/material/dialog';
import {MatSnackBar} from '@angular/material/snack-bar';
import {DeviceService} from '@app/services/device.service';
import {TableData} from '@app/models/table-data';

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class OverviewComponent implements OnInit {
  groups: any[];
  formattedGroups: TableData[];
  utcNow: number;
  constructor(private groupService: GroupService, private deviceService: DeviceService,
              public dialog: MatDialog, public snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.loadGroups();
  }

  loadGroups(): void{
    this.groupService.loadGroupsWithDevices(4).subscribe(results => {
      this.utcNow = new Date().getTime();
      // refresh the result
      this.groups = results.map(v => {
        v.devices.map(d => {
          const temp = d.licenses.sort((l1, l2) => {
            const exp1 = new Date(l1.expireOn).getTime();
            const exp2 = new Date(l2.expireOn).getTime();
            if (exp1 > exp2){
              return 1;
            }
            else if (exp1 < exp2){
              return -1;
            }
            else{
              return 0;
            }
          });
          d.license = temp[0];

          if (!d.lastStatus){
            d.isOnline = false;
          }
          else{
            const lastStatusTime = new Date(d.lastStatus.updatedOn).getTime();
            // check last 15 minutes to decide if the device is online or offline
            d.isOnline = (this.utcNow - 15 * 60000) > lastStatusTime;
          }
        });
        return v;
      });

      this.formattedGroups = this.groups.map(g => {
        const data: TableData = {
          title: g.name,
          template: 'deviceTemplate',
          headerRow: ['Status', 'Serial Number', 'Asset Tag', 'License'],
          dataRows: g.devices
        };
        return data;
      });
    });
  }

  toggle(item): void{
    item.checked = !item.checked;
  }
}
