import { StateDto } from '@/api/state';
import { VirtualStateDto } from '@/api/market';
import { AssetTagStateDto, DashboardConfigStateDto, DashboardStateDto } from '@/api/meta';

export default class AppState {
	activeState: StateDto = {
		brokers: [],
	};
	selectedVirtualStatePeriod = 'AllTime';
	selectedVirtualStateBroker = '';
	virtualState: VirtualStateDto = {
		balances: [],
	};
	tagState: AssetTagStateDto = {
		assets: [],
	};
	dashboardConfigState: DashboardConfigStateDto = {
		dashboards: [],
	};
	selectedDashboardState: DashboardStateDto | undefined;
}
