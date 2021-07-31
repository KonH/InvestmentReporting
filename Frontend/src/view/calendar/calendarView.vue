<template>
	<calendar-currency-view
		v-for="currency in currencies"
		:key="currency"
		:currency-code="currency"
		:operations="getCurrencyOperations(currency)"
		:assets="getCurrencyAssets(currency)"
	/>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import Backend from '@/service/backend';
import { OperationDto } from '@/api/state';
import { VirtualStateDto } from '@/api/market';
import CalendarCurrencyView from '@/view/calendar/calendarCurrencyView.vue';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'CalendarView',
	components: {
		CalendarCurrencyView,
	},
})
export default class CalendarView extends Vue {
	@Prop()
	targetCategory!: string;

	@State('virtualState')
	virtualState!: VirtualStateDto;

	allOperations: OperationDto[] = [];

	async created() {
		const startDate = new Date(1, 1, 1).toISOString();
		const endDate = new Date().toISOString();
		const response = await Backend.state().operation.operationList({
			startDate: startDate,
			endDate: endDate,
		});
		this.allOperations = response.data;
	}

	get dividendOperations() {
		return this.allOperations.filter((o) => o.category == this.targetCategory);
	}

	get currencies() {
		return this.virtualState.balances?.map((b) => b.currency) ?? [];
	}

	getCurrencyOperations(currencyCode: string) {
		return this.dividendOperations.filter((o) => o.currency == currencyCode);
	}

	getCurrencyAssets(currencyCode: string) {
		const balances = this.virtualState.balances;
		if (!balances) {
			return [];
		}
		const currencyBalance = balances.find((b) => b.currency == currencyCode);
		if (!currencyBalance) {
			return [];
		}
		return currencyBalance.inventory ?? [];
	}
}
</script>
