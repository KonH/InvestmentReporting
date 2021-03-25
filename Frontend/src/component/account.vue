<template>
	<b>{{ account.displayName }} ({{ balanceFormat }})</b>
	<router-link
		:to="`/addIncome/${brokerId}/${account.id}`"
		class="btn btn-primary ml-2"
		>Add Income</router-link
	>
	<router-link
		:to="`/addExpense/${brokerId}/${account.id}`"
		class="btn btn-primary ml-2"
		>Add Expense</router-link
	>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { AccountDto, CurrencyDto, StateDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';
import { State } from 'vuex-class';

@Options({
	name: 'Account',
})
export default class Account extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@Prop()
	brokerId!: string;

	@Prop()
	account!: AccountDto;

	get balanceFormat() {
		const accountCurrency = this.activeState.currencies?.find(
			(c) => c.id == this.account.currency
		) as CurrencyDto;
		const format = accountCurrency.format;
		return format?.replace('{0}', (this.account.balance ?? 0).toString());
	}
}
</script>
