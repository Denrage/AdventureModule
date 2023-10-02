import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ParticipantsComponent } from './admin/participants/participants.component';
import { AdventuresComponent } from './admin/adventures/adventures.component';

const routes: Routes = [
  { path: 'admin/participants', component: ParticipantsComponent },
  { path: 'admin/adventures', component: AdventuresComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
