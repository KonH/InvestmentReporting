<template>
	<b>{{ account.displayName }} ({{ balanceFormat }})</b>
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
	account!: AccountDto;

	get balanceFormat() {
		const accountCurrency = this.activeState.currencies?.find(
			(c) => c.id == this.account.currency
		) as CurrencyDto;
		const format = accountCurrency.format;
		return format.replace('{0}', this.account.balance);
	}
}
</script>
