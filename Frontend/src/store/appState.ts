import { StateDto } from '@/api/state';
import { VirtualStateDto } from '@/api/market';
import { AssetTagStateDto, DashboardConfigStateDto } from '@/api/meta';

export default class AppState {
	activeState: StateDto = {
		brokers: [],
	};
	virtualState: VirtualStateDto = {
		balances: [],
	};
	tagState: AssetTagStateDto = {
		assets: [],
	};
	dashboardConfigState: DashboardConfigStateDto = {
		dashboards: [],
	};
}
