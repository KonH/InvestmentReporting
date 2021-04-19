<template>
	<h3>Exchange</h3>
	Contains all balance & instruments price converted to each currency using last exchange price:
	<ul>
		<li v-for="(summary, currency) in virtualState.summary" :key="currency">
			<b>{{ currency }}</b
			>:
			<money :value="summary.virtualSum" :currency-code="currency" />
			<money-diff :old="summary.realSum" :new="summary.virtualSum" :currency-code="currency" class="ml-2" />
		</li>
	</ul>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';
import { StateDto } from '@/api/state';

@Options({
	name: 'PortfolioExchangeView',
	components: {
		Money,
		MoneyDiff,
	},
})
export default class PortfolioExchangeView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@State('virtualState')
	virtualState!: VirtualStateDto;
}
</script>
