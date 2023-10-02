import { Component } from '@angular/core';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.scss']
})
export class NavigationComponent {
  items: MenuItem[];

  constructor(){
    this.items = [
      {
        label: "Adventures",
        routerLink: "/admin/adventures"
      },
      {
        label: "Participants",
        routerLink: "/admin/participants"
      }
    ];
  }
}
