<template>
	<div>
		<h3>View</h3>
		<label>
			Currency:
			<select ref="currency" v-model="selectedCurrency" class="form-control">
				<option v-for="currency in currencies" :key="currency.code" :value="currency.code">
					{{ currency.code }}
				</option>
			</select>
		</label>
		<div class="row">
			<div class="col">
				<dashboard-legend :dashboard="selectedDashboardState" :dashboard-config="dashboardConfig" :currency-code="selectedCurrency" />
			</div>
			<div class="col">
				<dashboard-chart :dashboard="selectedDashboardState" :currency-code="selectedCurrency" />
			</div>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { DashboardConfigDto, DashboardStateDto } from '@/api/meta';
import { Prop } from 'vue-property-decorator';
import { Action, State } from 'vuex-class';
import DashboardLegend from '@/component/dashboardLegend.vue';
import { StateDto } from '@/api/state';
import DashboardChart from '@/component/dashboardChart.vue';

@Options({
	name: 'DashboardView',
	components: {
		DashboardLegend,
		DashboardChart,
	},
})
export default class DashboardView extends Vue {
	@Action('fetchDashboardState')
	fetchDashboardState!: (id: string) => void;

	@State('selectedDashboardState')
	selectedDashboardState!: DashboardStateDto;

	@State('activeState')
	activeState!: StateDto;

	@Prop()
	dashboardConfig!: DashboardConfigDto;

	selectedCurrency = '';

	created() {
		const currencies = this.activeState.currencies;
		if (!currencies || currencies.length == 0) {
			return;
		}
		const firstCode = currencies[0].code;
		if (!firstCode) {
			return;
		}
		this.selectedCurrency = firstCode;
	}

	get currencies() {
		return this.activeState.currencies;
	}
}
</script>
