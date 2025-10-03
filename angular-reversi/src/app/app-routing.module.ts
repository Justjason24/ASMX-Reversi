import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ReversiComponent } from './reversi/reversi.component';

const routes: Routes = [
    { path: 'reversi', component: ReversiComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
