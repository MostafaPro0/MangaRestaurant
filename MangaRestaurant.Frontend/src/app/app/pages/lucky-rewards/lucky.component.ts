import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LuckyRewardsComponent } from '../../components/lucky-rewards/lucky-rewards.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-lucky-rewards-page',
  standalone: true,
  imports: [CommonModule, LuckyRewardsComponent, TranslateModule],
  templateUrl: './lucky.component.html',
  styleUrl: './lucky.component.css'
})
export class LuckyRewardsPageComponent {
}
