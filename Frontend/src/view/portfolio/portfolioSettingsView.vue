<template>
	<div class="form-group">
		<label>
			Period:
			<select v-model="selectedPeriod" class="form-control">
				<option selected>AllTime</option>
				<option>CalendarYear</option>
				<option>RollingYear</option>
				<option>CalendarMonth</option>
				<option>RollingMonth</option>
				<option>CalendarWeek</option>
				<option>RollingWeek</option>
			</select>
		</label>
		<label class="ml-2">
			Broker:
			<select v-model="selectedBroker" class="form-control">
				<option v-for="broker in brokers" :key="broker.id" :value="broker.id">{{ broker.displayName }}</option>
			</select>
		</label>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action, State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import { BrokerDto, StateDto } from '@/api/state';

@Options({
	name: 'PortfolioSettingsView',
})
export default class PortfolioView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@State('virtualState')
	virtualState!: VirtualStateDto;

	@State('selectedVirtualStatePeriod')
	selectedVirtualStatePeriod!: string;

	@Action('changeSelectedVirtualStatePeriod')
	changeSelectedVirtualStatePeriod!: (period: string) => void;

	@Action('changeSelectedVirtualStateBroker')
	changeSelectedVirtualStateBroker!: (period: string) => void;

	selectedPeriod = '';

	selectedBroker = '';

	mounted() {
		this.selectedPeriod = this.selectedVirtualStatePeriod;
		this.selectedBroker = '';
		this.$watch('selectedPeriod', this.changeSelectedVirtualStatePeriod);
		this.$watch('selectedBroker', this.changeSelectedVirtualStateBroker);
	}

	get balances() {
		return this.virtualState.balances;
	}

	get brokers() {
		const brokers = this.activeState.brokers ?? [];
		const empty: BrokerDto[] = [
			{
				id: '',
				displayName: 'All',
			},
		];
		return empty.concat(brokers);
	}
}
</script>
