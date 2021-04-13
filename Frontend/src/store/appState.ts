import { StateDto } from '@/api/state';
import { VirtualStateDto } from '@/api/market';

export default class AppState {
	activeState: StateDto = {
		brokers: [],
	};
	virtualState: VirtualStateDto = {
		inventory: [],
	};
}
