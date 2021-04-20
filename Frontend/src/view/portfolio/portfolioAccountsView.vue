<template>
	<h3>Accounts</h3>
	Contains all unused balance for each account:
	<ul>
		<li v-for="broker in brokers" :key="broker.id">
			<b>{{ broker.displayName }}</b
			>:
			<ul>
				<li v-for="account in broker.accounts" :key="account.id">
					{{ account.displayName }}: <money :value="account.balance" :currency-code="account.currency" />
				</li>
			</ul>
		</li>
	</ul>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { State } from 'vuex-class';
import Money from '@/component/common/money.vue';
import MoneyDiff from '@/component/common/moneyDiff.vue';
import { StateDto } from '@/api/state';

@Options({
	name: 'PortfolioAccountsView',
	components: {
		Money,
		MoneyDiff,
	},
})
export default class PortfolioAccountsView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@State('selectedVirtualStateBroker')
	selectedVirtualStateBroker!: string;

	get brokers() {
		const allBrokers = this.activeState.brokers ?? [];
		const selectedId = this.selectedVirtualStateBroker;
		return allBrokers.filter((b) => !selectedId || b.id == selectedId);
	}
}
</script>
