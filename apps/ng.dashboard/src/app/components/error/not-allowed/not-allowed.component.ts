import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {Router} from '@angular/router';

@Component({
  selector: 'app-not-allowed',
  templateUrl: './not-allowed.component.html',
  styleUrls: ['./not-allowed.component.scss'],
  encapsulation: ViewEncapsulation.Emulated
})
export class NotAllowedComponent implements OnInit {
  seconds = 5;
  constructor(private router: Router) { }

  ngOnInit() {
    const t = setInterval(()=>{
      this.seconds--;
      if(this.seconds === 0){
        clearInterval(t);
        this.router.navigateByUrl('login');
      }
    }, 1000);
  }

}
