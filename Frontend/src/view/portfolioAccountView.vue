<template>
	<h3>Accounts</h3>
	Contains all unused balance for each account:
	<ul>
		<li v-for="broker in activeState.brokers" :key="broker.id">
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
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';
import { StateDto } from '@/api/state';

@Options({
	name: 'PortfolioAccountView',
	components: {
		Money,
		MoneyDiff,
	},
})
export default class PortfolioAccountView extends Vue {
	@State('activeState')
	activeState!: StateDto;
}
</script>
